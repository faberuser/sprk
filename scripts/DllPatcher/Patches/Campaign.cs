using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static MethodDefinition BuildSurvivorFilter(ModuleDefinition module, TypeDefinition owner, MethodReference add)
    {
        var helper = new MethodDefinition("SprkAddPartySurvivor", MethodAttributes.Public | MethodAttributes.Static, module.TypeSystem.Void);
        helper.Parameters.Add(new ParameterDefinition(add.DeclaringType));
        helper.Parameters.Add(new ParameterDefinition(module.TypeSystem.Int32));
        helper.Parameters.Add(new ParameterDefinition(new ArrayType(module.TypeSystem.Int32)));
        helper.Parameters.Add(new ParameterDefinition(new ArrayType(module.TypeSystem.Int32)));
        owner.Methods.Add(helper);
        var array = new TypeReference("System", "Array", module, module.TypeSystem.CoreLibrary);
        var indexOf = new MethodReference("IndexOf", module.TypeSystem.Int32, array);
        var generic = new GenericParameter("T", indexOf);
        indexOf.GenericParameters.Add(generic);
        indexOf.Parameters.Add(new ParameterDefinition(new ArrayType(generic)));
        indexOf.Parameters.Add(new ParameterDefinition(generic));
        var indexOfInt = new GenericInstanceMethod(indexOf);
        indexOfInt.GenericArguments.Add(module.TypeSystem.Int32);
        var contains = new MethodReference("Contains", module.TypeSystem.Boolean, add.DeclaringType) { HasThis = true };
        contains.Parameters.Add(new ParameterDefinition(add.Parameters[0].ParameterType));
        var il = helper.Body.GetILProcessor();
        var accepted = il.Create(OpCodes.Ldarg_0);
        var end = il.Create(OpCodes.Ret);
        foreach (int argument in new[] { 2, 3 })
        {
            var next = il.Create(OpCodes.Nop);
            il.Append(il.Create(OpCodes.Ldarg, helper.Parameters[argument]));
            il.Append(il.Create(OpCodes.Brfalse, next));
            il.Append(il.Create(OpCodes.Ldarg, helper.Parameters[argument]));
            il.Append(il.Create(OpCodes.Ldarg_1));
            il.Append(il.Create(OpCodes.Call, indexOfInt));
            il.Append(il.Create(OpCodes.Ldc_I4_0));
            il.Append(il.Create(OpCodes.Bge, accepted));
            il.Append(next);
        }
        il.Append(il.Create(OpCodes.Br, end));
        il.Append(accepted);
        il.Append(il.Create(OpCodes.Ldarg_1));
        il.Append(il.Create(OpCodes.Callvirt, contains));
        il.Append(il.Create(OpCodes.Brtrue, end));
        il.Append(il.Create(OpCodes.Ldarg_0));
        il.Append(il.Create(OpCodes.Ldarg_1));
        il.Append(il.Create(OpCodes.Callvirt, add));
        il.Append(end);
        return helper;
    }

    static void PatchCampaignSurvivors(ModuleDefinition module)
    {
        var context = module.Types.Single(t => t.FullName == "NGame2.NBattleContext.CampaignContext");
        if (context.Methods.Any(m => m.Name == "SprkAddPartySurvivor")) return;
        var endCampaign = context.Methods.Single(m => m.Name == "EndCampaign");
        var addInstruction = endCampaign.Body.Instructions.Single(i => i.Operand is MethodReference m
            && m.Name == "Add" && m.DeclaringType is GenericInstanceType g
            && g.ElementType.FullName == "System.Collections.Generic.List`1"
            && g.GenericArguments[0].FullName == "System.Int32");
        var helper = BuildSurvivorFilter(module, context, (MethodReference)addInstruction.Operand);
        // The native loop already checks HP, but includes creatures from both teams.
        // Pass only selected main/sub-party heroes, never surviving scenario NPCs.
        var il = endCampaign.Body.GetILProcessor();
        addInstruction.OpCode = OpCodes.Ldarg_0;
        addInstruction.Operand = null;
        var last = addInstruction;
        foreach (var instruction in new[] {
            il.Create(OpCodes.Ldfld, context.Fields.Single(f => f.Name == "HeroIndices")),
            il.Create(OpCodes.Ldarg_0),
            il.Create(OpCodes.Ldfld, context.Fields.Single(f => f.Name == "GroupHeroIndices")),
            il.Create(OpCodes.Call, helper) })
        { il.InsertAfter(last, instruction); last = instruction; }
        Console.WriteLine("Patched campaign survivor reports to include only selected party heroes");
    }

    static void TestCampaignSurvivors()
    {
        using var module = ModuleDefinition.CreateModule("SurvivorFilterTest", ModuleKind.Dll);
        var type = new TypeDefinition("", "SurvivorFilter", TypeAttributes.Public, module.TypeSystem.Object);
        module.Types.Add(type);
        var add = module.ImportReference(typeof(System.Collections.Generic.List<int>).GetMethod("Add")!);
        BuildSurvivorFilter(module, type, add);
        using var bytes = new MemoryStream();
        module.Write(bytes);
        var method = System.Reflection.Assembly.Load(bytes.ToArray()).GetType("SurvivorFilter")!.GetMethod("SprkAddPartySurvivor")!;
        var result = new System.Collections.Generic.List<int>();
        foreach (int id in new[] { 1, 99999, 4, 1, 3 })
            method.Invoke(null, new object?[] { result, id, new[] { 1, 2 }, new[] { 4 } });
        if (!result.SequenceEqual(new[] { 1, 4 })) throw new Exception("Invalid survivor filtering");
        method.Invoke(null, new object?[] { result, 7, null, null });
        method.Invoke(null, new object?[] { result, 8, null, new[] { 8 } });
        if (!result.SequenceEqual(new[] { 1, 4, 8 })) throw new Exception("Invalid null-party handling");
        Console.WriteLine("Survivor IL tests passed: main/sub parties, NPC rejection, duplicates, and null arrays");
    }
}
