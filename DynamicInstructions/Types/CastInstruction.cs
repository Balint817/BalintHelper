using DynamicInstructions.Abstract;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace DynamicInstructions.Types
{
    public static class CastCache
    {
        private static readonly ConcurrentDictionary<(Type Source, Type Target), Func<object, object>> _cache = new();

        public static bool TryGetCast(Type source, Type target, out Func<object, object> cast)
        {
            try
            {
                cast = _cache.GetOrAdd((source, target), static key =>
                {
                    var input = Expression.Parameter(typeof(object), "value");
                    var typedInput = Expression.Convert(input, key.Source);

                    Expression body = Expression.Convert(typedInput, key.Target);

                    var boxed = Expression.Convert(body, typeof(object));
                    return Expression.Lambda<Func<object, object>>(boxed, input).Compile();
                });

                return true;
            }
            catch (InvalidOperationException)
            {
                cast = null!;
                return false;
            }
        }
    }

    public class CastInstruction : BaseInstruction
    {
        public readonly Type TargetType;
        public readonly Type? SourceType;

        public CastInstruction(Type targetType, Type? sourceType = null)
        {
            TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
            SourceType = sourceType;
        }

        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPop(out var value))
            {
                throw new InvalidProgramException("stack imbalance, failed to cast value");
            }

            if (value is null)
            {
                if (TargetType.IsValueType && Nullable.GetUnderlyingType(TargetType) is null)
                {
                    throw new InvalidProgramException("cannot cast null to a non-nullable value type");
                }

                state.Stack.Push(null);
                return;
            }

            var sourceType = value.GetType();

            if (TargetType.IsAssignableFrom(sourceType))
            {
                state.Stack.Push(value);
                return;
            }

            if (!CastCache.TryGetCast(sourceType, TargetType, out var cast))
            {
                throw new InvalidProgramException($"No conversion exists from {sourceType} to {TargetType}.");
            }

            state.Stack.Push(cast(value));
        }
    }
}