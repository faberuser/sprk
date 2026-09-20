using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static void RestoreCostumeOwnership(ModuleDefinition module)
    {
        // Compile against the client's Mono libraries, not the patcher's .NET runtime.
        // Only method bodies are copied; no helper assembly is installed in the game.
        using var sourceStream = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.CostumeOwnership.cs.txt")!;
        using var reader = new StreamReader(sourceStream);
        var references = Directory.GetFiles(Path.GetDirectoryName(module.FileName)!, "*.dll")
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create("CostumeOwnershipRestore",
            new[] { CSharpSyntaxTree.ParseText(reader.ReadToEnd()) }, references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Release));
        using var compiled = new MemoryStream();
        var result = compilation.Emit(compiled);
        if (!result.Success)
            throw new InvalidOperationException("Cannot compile native costume checks:\n" +
                string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        compiled.Position = 0;
        using var original = AssemblyDefinition.ReadAssembly(compiled);
        int restored = 0;
        foreach (var sourceType in original.MainModule.Types.Where(t => t.Namespace == "NGame2.NUtil.NCostume"))
        {
            var targetType = module.Types.Single(t => t.FullName == sourceType.FullName);
            foreach (var source in sourceType.Methods.Where(m => m.Name == "GetCostumeState"))
            {
                var target = targetType.Methods.Single(m => m.FullName == source.FullName);
                var body = new MethodBody(target) { InitLocals = source.Body.InitLocals, MaxStackSize = source.Body.MaxStackSize };
                target.Body = body;
                foreach (var variable in source.Body.Variables)
                    body.Variables.Add(new VariableDefinition(module.ImportReference(variable.VariableType)));
                var instructions = source.Body.Instructions.ToDictionary(i => i, i => Instruction.Create(OpCodes.Nop));
                foreach (var instruction in source.Body.Instructions)
                {
                    var copy = instructions[instruction];
                    copy.OpCode = instruction.OpCode;
                    copy.Operand = instruction.Operand switch
                    {
                        Instruction branch => instructions[branch],
                        Instruction[] branches => branches.Select(b => instructions[b]).ToArray(),
                        VariableDefinition variable => body.Variables[variable.Index],
                        ParameterDefinition parameter => target.Parameters[parameter.Index],
                        MethodReference method when method.DeclaringType.Scope == original.MainModule =>
                            module.Types.Single(t => t.FullName == method.DeclaringType.FullName)
                                .Methods.Single(m => m.FullName == method.FullName),
                        MethodReference method => module.ImportReference(method),
                        FieldReference field => module.ImportReference(field),
                        TypeReference type => module.ImportReference(type),
                        null => null,
                        var operand => operand
                    };
                    body.Instructions.Add(copy);
                }
                if (source.Body.HasExceptionHandlers)
                    throw new InvalidOperationException("Unexpected exception handler in costume ownership source");
                restored++;
            }
        }
        if (restored != 10)
            throw new InvalidOperationException($"Expected 10 native costume state methods, restored {restored}");
        if (module.AssemblyReferences.Any(r => r.Name == "CostumeOwnershipRestore"))
            throw new InvalidOperationException("Costume restoration leaked a helper assembly reference");
        Console.WriteLine($"Restored {restored} native costume ownership/equipment/purchase checks");
    }
}
