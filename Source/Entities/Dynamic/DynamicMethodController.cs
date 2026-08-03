using Celeste.Mod.BalintHelper.Triggers.Dynamic;
using DynamicInstructions;
using DynamicInstructions.Instructions;
using DynamicInstructions.Instructions.Abstract;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.BalintHelper.Entities.Dynamic
{
    [Tracked(false)]
    public class DynamicMethodController : Entity
    {
        private const BindingFlags AllConstructorFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public static BaseInstructionTrigger TemporaryReturnTrigger()
        {
            var returnTrigger = new BaseInstructionTrigger(new EntityData()
            {
                Name = "BalintHelper/BaseInstructionTrigger/ReturnInstruction",

            }, Vector2.Zero);
            return returnTrigger;
        }
        public static BaseInstructionTrigger TemporaryNopTrigger()
        {
            var nopTrigger = new BaseInstructionTrigger(new EntityData()
            {
                Name = "BalintHelper/BaseInstructionTrigger/NopInstruction"
            }, Vector2.Zero);
            return nopTrigger;
        }
        public static BaseInstructionTrigger TemporaryLoadBoolTrigger()
        {
            var returnTrigger = new LoadPrimitiveInstructionTrigger(new EntityData()
            {
                Name = "BalintHelper/LoadPrimitiveInstructionTrigger/LoadConstant",
                Values = new()
                {
                    ["constantType"] = "Bool",
                    ["value"] = "true"
                }
            }, Vector2.Zero);
            return returnTrigger;
        }
        public static BaseInstructionTrigger TemporaryConditionalTrigger()
        {
            var returnTrigger = new ConditionalInstructionTrigger(new EntityData()
            {
                Name = "BalintHelper/ConditionalInstructionTrigger/ConditionalInstruction"
            }, Vector2.Zero);
            return returnTrigger;
        }
        public static DynamicMethodController GetOrCreate(Scene scene)
        {
            var existing = scene.Tracker.GetEntity<DynamicMethodController>();
            if (existing is not null)
            {
                return existing;
            }
            var controller = new DynamicMethodController(new(), Vector2.Zero);
            scene.Add(controller);
            return controller;
        }
        public readonly Interpreter Interpreter;
        public readonly ReadOnlyCollection<Assembly> Assemblies;
        public readonly ReadOnlyCollection<Type> AllTypes;
        public readonly ReadOnlyCollection<Type> InstructionTypes;
        public DynamicMethodController(EntityData data, Vector2 offset) : base(offset)
        {
            Interpreter = new();
            Assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList().AsReadOnly();
            AllTypes = Assemblies.SelectMany(TypeNameCodec.GetLoadableTypes).ToList().AsReadOnly();
            InstructionTypes = AllTypes.Where(type => type.IsAssignableTo(typeof(BaseInstruction))).ToList().AsReadOnly();
        }

        private Type? _returnInstructionType;

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            LoadAll();
        }

        public void LoadAll()
        {
            var instructionTriggers = Scene.Entities.Where(x => x is BaseInstructionTrigger)
                .Cast<BaseInstructionTrigger>()
                .ToList();

            var defineTriggers = Scene.Tracker.GetEntities<DefineMethodTrigger>()
                .Cast<DefineMethodTrigger>()
                .ToArray();

            // Every real instruction may belong to exactly one method. Filled in while laying methods
            // out, so a fallthrough/jump that reaches foreign code is reported instead of silently
            // duplicating (or corrupting) instructions.
            var owners = new Dictionary<BaseInstructionTrigger, DefineMethodTrigger>();

            var layouts = new List<BaseInstructionTrigger>[defineTriggers.Length];
            for (int i = 0; i < defineTriggers.Length; i++)
            {
                layouts[i] = LayoutMethod(defineTriggers[i], instructionTriggers, owners);
            }

            for (int i = 0; i < defineTriggers.Length; i++)
            {
                // Compile mutates the layout list (it may splice in ghost labels), so it has to run
                // before the body array is taken.
                var body = Compile(layouts[i], defineTriggers[i]);
                Interpreter.RegisterDynamicMethod(defineTriggers[i].MethodName, body, defineTriggers[i].ArgCount);
            }
        }

        /// <summary>
        /// Flattens the instruction graph reachable from <paramref name="define"/> into the linear
        /// order the interpreter executes, inserting ghost instructions wherever the graph cannot be
        /// expressed by plain fallthrough:
        /// <list type="bullet">
        /// <item>a chain that runs out of successors gets a ghost <c>ReturnInstruction</c>, so the
        /// block that is appended after it is never entered by accident;</item>
        /// <item>an unconditional edge into already emitted code (i.e. a loop) becomes a ghost
        /// <c>LoadConstant(true)</c> + ghost <c>ConditionalInstruction</c> pair that jumps there.</item>
        /// </list>
        /// </summary>
        private List<BaseInstructionTrigger> LayoutMethod(
            DefineMethodTrigger define,
            List<BaseInstructionTrigger> allTriggers,
            Dictionary<BaseInstructionTrigger, DefineMethodTrigger> owners)
        {
            var order = new List<BaseInstructionTrigger>();
            var placed = new HashSet<BaseInstructionTrigger>();

            // Block leaders that still have to be appended (true paths of conditionals).
            var pending = new Queue<BaseInstructionTrigger>();

            var entry = FirstTargetOf(define, allTriggers)
                ?? throw new InvalidOperationException(
                    $"DefineMethodTrigger '{define.MethodName}' at {define.Position} has no node pointing at an instruction.");

            pending.Enqueue(entry);

            while (pending.Count > 0)
            {
                var current = pending.Dequeue();

                // Reached mid-chain by a fallthrough in the meantime.
                if (placed.Contains(current))
                {
                    continue;
                }

                while (true)
                {
                    if (placed.Contains(current))
                    {
                        // Unconditional edge into code that already lives somewhere else in the array
                        // (backwards jump / re-entry). The array has no goto, so synthesise one:
                        // push `true`, then branch on it.
                        var loadTrue = Ghost(TemporaryLoadBoolTrigger());
                        var jump = (ConditionalInstructionTrigger)Ghost(TemporaryConditionalTrigger());
                        jump.TruePath = current;

                        order.Add(loadTrue);
                        order.Add(jump);
                        break;
                    }

                    Claim(current, define, owners);
                    order.Add(current);
                    placed.Add(current);

                    var successors = TargetsOf(current, allTriggers);
                    BaseInstructionTrigger? next;

                    if (current is ConditionalInstructionTrigger conditional)
                    {
                        if (successors.Count < 2)
                        {
                            throw new InvalidOperationException(
                                $"ConditionalInstructionTrigger at {conditional.Position} (method '{define.MethodName}') needs two nodes "
                                + $"colliding with instructions: the first is the false/fallthrough path, the second is the true path "
                                + $"(found {successors.Count}).");
                        }

                        next = successors[0];
                        var target = successors[1];

                        // A branch may never leave its method.
                        Claim(target, define, owners);
                        conditional.TruePath = target;

                        // The true path is laid out as its own block; it is terminated by a ghost
                        // return (or a ghost jump) so the preceding block cannot fall into it.
                        if (!placed.Contains(target))
                        {
                            pending.Enqueue(target);
                        }
                    }
                    else
                    {
                        next = successors.Count > 0 ? successors[0] : null;
                    }

                    if (next is null)
                    {
                        if (!IsReturn(current))
                        {
                            order.Add(Ghost(TemporaryReturnTrigger()));
                        }
                        break;
                    }

                    current = next;
                }
            }

            return order;
        }

        /// <summary>
        /// Instantiates the <see cref="BaseInstruction"/> for every trigger in the layout.
        /// Conditionals take their target instance through the constructor, so they are built in
        /// dependency order. A cycle of conditionals branching into each other is broken by splicing
        /// a ghost <c>NopInstruction</c> in front of the target and branching to that label instead.
        /// </summary>
        private BaseInstruction[] Compile(List<BaseInstructionTrigger> order, DefineMethodTrigger define)
        {
            var compiled = new Dictionary<BaseInstructionTrigger, BaseInstruction>();
            var unresolved = new List<ConditionalInstructionTrigger>();

            foreach (var trigger in order)
            {
                if (trigger is ConditionalInstructionTrigger conditional)
                {
                    unresolved.Add(conditional);
                }
                else
                {
                    compiled[trigger] = Instantiate(trigger, define);
                }
            }

            while (unresolved.Count > 0)
            {
                var progressed = false;

                for (int i = unresolved.Count - 1; i >= 0; i--)
                {
                    var conditional = unresolved[i];
                    var target = conditional.TruePath
                        ?? throw new InvalidOperationException(
                            $"ConditionalInstructionTrigger at {conditional.Position} (method '{define.MethodName}') has no true path.");

                    if (!compiled.TryGetValue(target, out var targetInstruction))
                    {
                        continue;
                    }

                    conditional.TruePathCompiled = targetInstruction;
                    compiled[conditional] = Instantiate(conditional, define);
                    unresolved.RemoveAt(i);
                    progressed = true;
                }

                if (progressed)
                {
                    continue;
                }

                // Only conditionals branching into other, still unbuilt conditionals are left.
                var blocked = unresolved[0].TruePath!;
                var index = order.IndexOf(blocked);
                if (index < 0)
                {
                    throw new InvalidOperationException(
                        $"true path target of the conditional at {unresolved[0].Position} is not part of method '{define.MethodName}'.");
                }

                var label = Ghost(TemporaryNopTrigger());
                order.Insert(index, label);
                compiled[label] = Instantiate(label, define);

                foreach (var conditional in unresolved)
                {
                    if (conditional.TruePath == blocked)
                    {
                        conditional.TruePath = label;
                    }
                }
            }

            var unwrapAndUncompound = new List<BaseInstruction>(order.Count);

            foreach (var trigger in order)
            {
                unwrapAndUncompound.Add(compiled[trigger]);
                if (trigger is CompoundInstructionTrigger compound)
                {
                    unwrapAndUncompound.AddRange(compound.GetCompoundInstructions());
                }
            }

            return [.. unwrapAndUncompound];
        }

        private static BaseInstruction Instantiate(BaseInstructionTrigger trigger, DefineMethodTrigger define)
        {
            var type = trigger.InstructionType
                ?? throw new InvalidOperationException(
                    $"instruction trigger at {trigger.Position} (method '{define.MethodName}') has no resolved instruction type.");

            var constructor = type.GetConstructor(AllConstructorFlags, null, trigger.ConstructorParameterTypes, null)
                ?? throw new InvalidOperationException(
                    $"{type.Name} has no constructor taking ({string.Join(", ", trigger.ConstructorParameterTypes.Select(x => x.Name))}).");

            return (BaseInstruction)constructor.Invoke(trigger.GetConstructorParameters());
        }

        /// <summary>
        /// Prepares a synthesised trigger: it never enters the scene (and therefore never the
        /// tracker), but <see cref="Entity.Added"/> is what resolves its instruction type and
        /// parameters, so it is invoked manually.
        /// </summary>
        private T Ghost<T>(T trigger) where T : BaseInstructionTrigger
        {
            trigger.Added(Scene);
            return trigger;
        }

        private bool IsReturn(BaseInstructionTrigger trigger)
        {
            _returnInstructionType ??= Ghost(TemporaryReturnTrigger()).InstructionType;
            return trigger.InstructionType is not null && trigger.InstructionType == _returnInstructionType;
        }

        private static void Claim(
            BaseInstructionTrigger trigger,
            DefineMethodTrigger define,
            Dictionary<BaseInstructionTrigger, DefineMethodTrigger> owners)
        {
            if (owners.TryGetValue(trigger, out var owner))
            {
                if (owner != define)
                {
                    throw new InvalidOperationException(
                        $"instruction at {trigger.Position} is reached by both '{owner.MethodName}' and '{define.MethodName}'; "
                        + "instructions cannot be shared between methods and branches cannot cross method boundaries.");
                }
                return;
            }
            owners[trigger] = define;
        }

        private static BaseInstructionTrigger? FirstTargetOf(
            Trigger trigger,
            List<BaseInstructionTrigger> allTriggers)
        {
            return TargetsOf(trigger, allTriggers).FirstOrDefault();
        }

        private static List<BaseInstructionTrigger> TargetsOf(
            Trigger trigger,
            List<BaseInstructionTrigger> allTriggers)
        {
            var targets = new List<BaseInstructionTrigger>();
            var data = trigger.SourceData;
            if (data?.Nodes is null)
            {
                return targets;
            }
            foreach (var node in data.NodesOffset(-data.Position + trigger.Position))
            {
                var hit = allTriggers.FirstOrDefault(t => t.CollidePoint(node));
                if (hit is not null)
                {
                    targets.Add(hit);
                }
            }
            return targets;
        }
    }

}
