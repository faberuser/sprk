using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static void PatchPortal(ModuleDefinition module)
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.Portal.cs.txt")!;
        using var reader = new StreamReader(stream);
        var references = Directory.GetFiles(Path.GetDirectoryName(module.FileName)!, "*.dll")
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create("PortalRestore",
            new[] { CSharpSyntaxTree.ParseText(reader.ReadToEnd()) }, references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));
        using var compiled = new MemoryStream();
        var result = compilation.Emit(compiled);
        if (!result.Success) throw new InvalidOperationException(string.Join("\n", result.Diagnostics));
        compiled.Position = 0;
        using var sourceAssembly = AssemblyDefinition.ReadAssembly(compiled);
        var sourceType = sourceAssembly.MainModule.Types.Single(t => t.Name == "RestoredPortal");
        var targetType = module.Types.SingleOrDefault(t => t.Name == sourceType.Name);
        if (targetType == null)
        {
            targetType = new TypeDefinition("", sourceType.Name, sourceType.Attributes, module.TypeSystem.Object);
            module.Types.Add(targetType);
        }
        foreach (var source in sourceType.Methods)
        {
            if (targetType.Methods.Any(m => m.Name == source.Name)) continue;
            var target = new MethodDefinition(source.Name, source.Attributes, module.ImportReference(source.ReturnType));
            foreach (var parameter in source.Parameters)
                target.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, module.ImportReference(parameter.ParameterType)));
            targetType.Methods.Add(target);
        }
        foreach (var source in sourceType.Methods)
        {
            var target = targetType.Methods.Single(m => m.Name == source.Name);
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
                    MethodReference method when method.DeclaringType.FullName == sourceType.FullName => targetType.Methods.Single(m => m.Name == method.Name),
                    MethodReference method => module.ImportReference(method),
                    FieldReference field => module.ImportReference(field),
                    TypeReference type => module.ImportReference(type),
                    var operand => operand
                };
                body.Instructions.Add(copy);
            }
            foreach (var handler in source.Body.ExceptionHandlers)
                body.ExceptionHandlers.Add(new ExceptionHandler(handler.HandlerType) {
                    TryStart = instructions[handler.TryStart], TryEnd = handler.TryEnd == null ? null : instructions[handler.TryEnd],
                    HandlerStart = instructions[handler.HandlerStart], HandlerEnd = handler.HandlerEnd == null ? null : instructions[handler.HandlerEnd],
                    CatchType = handler.CatchType == null ? null : module.ImportReference(handler.CatchType),
                    FilterStart = handler.FilterStart == null ? null : instructions[handler.FilterStart]
                });
        }
        var container = module.Types.Single(t => t.FullName == "NShared.PortalRenewalDataContainer");
        foreach (var pair in new[] { ("GetCategoryDatas", "Categories"), ("GetData", "Contents") })
        {
            var target = container.Methods.Single(m => m.Name == pair.Item1);
            var helper = targetType.Methods.Single(m => m.Name == pair.Item2);
            if (target.Body.Instructions.Any(i => i.Operand is MethodReference m && m.FullName == helper.FullName)) continue;
            // Mutate the return itself so existing branches to it also pass through the hook.
            foreach (var ret in target.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                var il = target.Body.GetILProcessor();
                ret.OpCode = pair.Item1 == "GetData" ? OpCodes.Ldarg_1 : OpCodes.Nop;
                il.InsertAfter(ret, Instruction.Create(OpCodes.Call, helper));
                il.InsertAfter(ret.Next, Instruction.Create(OpCodes.Ret));
            }
        }
        if (module.AssemblyReferences.Any(r => r.Name == "PortalRestore"))
            throw new InvalidOperationException("Portal patch leaked a helper assembly reference");
        Console.WriteLine("Restored 15 missing Portal categories and their native navigation.");
    }
}
