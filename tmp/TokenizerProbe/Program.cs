using System;
using System.Linq;
using System.Reflection;
using Microsoft.ML.Tokenizers;

class Program
{
    static void Main()
    {
        var asm = typeof(Tokenizer).Assembly;
        Console.WriteLine($"Assembly: {asm.FullName}");
        var types = asm.GetTypes().Where(t => t.IsPublic).OrderBy(t => t.FullName).ToList();
        foreach (var t in types)
        {
            Console.WriteLine(t.FullName);
            var methods = t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                           .Where(m => !m.IsSpecialName)
                           .ToList();
            foreach (var m in methods)
            {
                var parms = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name));
                Console.WriteLine($"  - {m.Name}({parms}) : {m.ReturnType.Name}");
            }
        }
    }
}
