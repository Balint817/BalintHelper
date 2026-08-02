using DynamicInstructions.Instructions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TestApp
{
    internal class Program
    {
        public static Action? test;
        static void Main()
        {
            foreach (var item in typeof(Program).GetNestedTypes(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public))
            {
                Console.WriteLine(item);
            }
            Console.WriteLine("Hello, World!");
            var type = TypeNameCodec.ParseType("System.Collections.Generic.List<System.Collections.Generic.List<string>>", AppDomain.CurrentDomain.GetAssemblies());
            Console.WriteLine(type);

            var nameInternal = "TestApp.Program+\\<\\>c__DisplayClass1_0";
            type = TypeNameCodec.ParseType(nameInternal, AppDomain.CurrentDomain.GetAssemblies());
            Console.WriteLine(type);

            string x = "5";
            test = () => Console.WriteLine(x);

            
        }
    }
}
