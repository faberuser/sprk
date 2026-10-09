using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static void PatchAdventureUnlocks(ModuleDefinition module)
    {
        using var input = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.AdventureUnlocks.cs.txt")!;
        using var reader = new StreamReader(input);
        var references = Directory.GetFiles(Path.GetDirectoryName(module.FileName)!, "*.dll")
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create("AdventureUnlockPatch",
            new[] { CSharpSyntaxTree.ParseText(reader.ReadToEnd()) }, references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));
        using var bytes = new MemoryStream();
        var result = compilation.Emit(bytes);
        if (!result.Success) throw new InvalidOperationException(string.Join("\n", result.Diagnostics));
        bytes.Position = 0;
        using var helper = AssemblyDefinition.ReadAssembly(bytes);
        var sourceType = helper.MainModule.Types.Single(t => t.Name == "SprkAdventureUnlocks");
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
            target.Body = new MethodBody(target) { InitLocals = source.Body.InitLocals, MaxStackSize = source.Body.MaxStackSize };
            foreach (var variable in source.Body.Variables)
                target.Body.Variables.Add(new VariableDefinition(module.ImportReference(variable.VariableType)));
            var instructions = source.Body.Instructions.ToDictionary(i => i, i => Instruction.Create(OpCodes.Nop));
            foreach (var instruction in source.Body.Instructions)
            {
                var copy = instructions[instruction];
                copy.OpCode = instruction.OpCode;
                copy.Operand = instruction.Operand switch
                {
                    Instruction branch => instructions[branch],
                    Instruction[] branches => branches.Select(b => instructions[b]).ToArray(),
                    VariableDefinition variable => target.Body.Variables[variable.Index],
                    ParameterDefinition parameter => target.Parameters[parameter.Index],
                    MethodReference call when call.DeclaringType.FullName == sourceType.FullName => targetType.Methods.Single(m => m.Name == call.Name),
                    MethodReference call => module.ImportReference(call),
                    FieldReference field => module.ImportReference(field),
                    TypeReference type => module.ImportReference(type),
                    var operand => operand
                };
                target.Body.Instructions.Add(copy);
            }
            if (source.Body.ExceptionHandlers.Count != 0) throw new InvalidOperationException("Unlock helper must not contain exception handlers");
        }

        var portal = module.GetType("NGame2.NUI.NHelper.NPortal.PortalHelper").Methods.Single(m => m.Name == "GetLockedReason");
        var click = module.GetType("NGame2.NUI.NWindow.PortalRenewal").Methods.Single(m => m.Name.EndsWith(".OnClickContentsButton"));
        var tutorial = targetType.Methods.Single(m => m.Name == "PortalTutorial");
        foreach (var method in new[] { portal, click })
        {
            foreach (var instruction in method.Body.Instructions.Where(i => i.Operand is MethodReference call
                && call.DeclaringType.FullName == "NShared.UserTutorialManager" && call.Name == "IsComplete"))
            {
                instruction.OpCode = OpCodes.Call;
                instruction.Operand = tutorial;
            }
            if (method.Body.Instructions.Count(i => i.Operand is MethodReference call && call.FullName == tutorial.FullName) != 1)
                throw new InvalidOperationException("Expected one Portal tutorial gate in " + method.FullName);
        }
        var category = module.GetType("NGame2.NUI.NManager.Mission.MissionManagement").Methods
            .Single(m => m.Name == "IsOpenSubCategory" && m.Parameters.Count == 2);
        var categoryHelper = targetType.Methods.Single(m => m.Name == "MissionCategory");
        if (!category.Body.Instructions.Any(i => i.Operand is MethodReference call && call.FullName == categoryHelper.FullName))
        {
            var il = category.Body.GetILProcessor();
            foreach (var ret in category.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                // Keep existing branches to ret valid; their bool is already on the stack.
                ret.OpCode = OpCodes.Ldarg_0;
                var sub = Instruction.Create(OpCodes.Ldarg_1);
                var call = Instruction.Create(OpCodes.Call, categoryHelper);
                il.InsertAfter(ret, sub);
                il.InsertAfter(sub, call);
                il.InsertAfter(call, Instruction.Create(OpCodes.Ret));
            }
            category.Body.MaxStackSize = Math.Max(3, category.Body.MaxStackSize);
        }
        if (module.AssemblyReferences.Any(r => r.Name == "AdventureUnlockPatch"))
            throw new InvalidOperationException("Unlock patch leaked a helper assembly reference");
        Console.WriteLine("Adventure entry gates honor tutorial skip and completed automatic dungeon milestones.");
    }
}
