using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllDisasm
{
    class Program
    {
        static void Main(string[] args)
        {
            string clientRoot = args.Length > 0
                ? args[0].TrimEnd('\\', '/')
                : PromptForClientPath();

            string managedDir = Path.Combine(clientRoot, "King's Raid_Data", "Managed");
            string dllPath = Path.Combine(managedDir, "Assembly-CSharp.dll");
            
            Console.WriteLine($"Loading: {dllPath}");
            
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(Path.GetDirectoryName(dllPath)!);
            
            var readerParams = new ReaderParameters { 
                ReadWrite = false,
                AssemblyResolver = resolver
            };
            
            using (var assembly = AssemblyDefinition.ReadAssembly(dllPath, readerParams))
            {
                var module = assembly.MainModule;
                
                var buttonGroupType = module.Types.FirstOrDefault(t => t.FullName == "NGame2.NUI.NWindow.HeroInnViewButtonGroup");
                if (buttonGroupType == null)
                {
                    Console.WriteLine("HeroInnViewButtonGroup not found!");
                    return;
                }
                
                Console.WriteLine($"\nFound: {buttonGroupType.FullName}");
                Console.WriteLine($"Methods: {buttonGroupType.Methods.Count}");
                
                foreach (var method in buttonGroupType.Methods)
                {
                    if (method.Name == "SetListener" || method.Name == "HideUnsupportedButtons")
                    {
                        Console.WriteLine($"\n=== {method.Name} ===");
                        if (method.HasBody)
                        {
                            Console.WriteLine($"Variables: {method.Body.Variables.Count}");
                            Console.WriteLine($"Instructions: {method.Body.Instructions.Count}");
                            foreach (var instr in method.Body.Instructions)
                            {
                                string operandStr = "";
                                if (instr.Operand != null)
                                {
                                    if (instr.Operand is FieldReference fr)
                                        operandStr = $"{fr.DeclaringType.Name}::{fr.Name}";
                                    else if (instr.Operand is MethodReference mr)
                                        operandStr = $"{mr.DeclaringType.Name}::{mr.Name}";
                                    else if (instr.Operand is Instruction target)
                                        operandStr = $"IL_{target.Offset:X4}";
                                    else
                                        operandStr = instr.Operand.ToString()!;
                                }
                                Console.WriteLine($"  IL_{instr.Offset:X4}: {instr.OpCode.Name,-12} {operandStr}");
                            }
                        }
                    }
                }
                
                // Also check HeroInnViewButton to see ActionType values used
                var buttonType = module.Types.FirstOrDefault(t => t.FullName == "NGame2.NUI.NWindow.HeroInnViewButton");
                if (buttonType != null)
                {
                    Console.WriteLine($"\n=== HeroInnViewButton Fields ===");
                    foreach (var field in buttonType.Fields)
                    {
                        Console.WriteLine($"  {field.Name}: {field.FieldType}");
                    }
                }
            }
        }

        static string PromptForClientPath()
        {
            string defaultPath = @"D:\client";
            Console.Write($"Enter game client path [default: {defaultPath}]: ");
            string? input = Console.ReadLine()?.Trim().TrimEnd('\\', '/');
            if (string.IsNullOrEmpty(input))
            {
                input = defaultPath;
            }
            Console.WriteLine($"Using client path: {input}");
            return input;
        }
    }
}