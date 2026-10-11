using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program
{
    static void PatchAccessoryShopDuplicates(ModuleDefinition module)
    {
        var type = module.GetType("NGame2.NUI.NComponent2.NPayShop.PayShopAccessoryList");
        var method = type.Methods.Single(m => m.Name == "FilteringData");
        var accessory = module.GetType("NShared.AccessoryCostumeData");
        var listType = (GenericInstanceType)method.ReturnType;
        MethodReference ListMethod(string name, TypeReference result, params TypeReference[] arguments)
        {
            var reference = new MethodReference(name, result, listType) { HasThis = true };
            foreach (var argument in arguments) reference.Parameters.Add(new ParameterDefinition(argument));
            return reference;
        }
        var element = listType.ElementType.GenericParameters[0];
        var constructor = ListMethod(".ctor", module.TypeSystem.Void);
        var count = ListMethod("get_Count", module.TypeSystem.Int32);
        var item = ListMethod("get_Item", element, module.TypeSystem.Int32);
        var add = ListMethod("Add", module.TypeSystem.Void, element);
        var getIndex = accessory.Methods.Single(m => m.Name == "get_Index");
        var isBuy = accessory.Methods.Single(m => m.Name == "get_IsBuy");

        // Category caches can contain the same accessory more than once after
        // table loading. Filter by its stable ownership/purchase ID, never by
        // display name or art: distinct variants must remain separate items.
        method.Body = new MethodBody(method) { InitLocals = true };
        var output = new VariableDefinition(listType);
        var inputIndex = new VariableDefinition(module.TypeSystem.Int32);
        var outputIndex = new VariableDefinition(module.TypeSystem.Int32);
        var data = new VariableDefinition(accessory);
        foreach (var variable in new[] { output, inputIndex, outputIndex, data }) method.Body.Variables.Add(variable);
        var il = method.Body.GetILProcessor();
        var inputCheck = Instruction.Create(OpCodes.Ldloc, inputIndex);
        var inputBody = Instruction.Create(OpCodes.Ldarg_0);
        var outputCheck = Instruction.Create(OpCodes.Ldloc, outputIndex);
        var outputBody = Instruction.Create(OpCodes.Ldloc, output);
        var next = Instruction.Create(OpCodes.Ldloc, inputIndex);
        var done = Instruction.Create(OpCodes.Ldloc, output);
        il.Emit(OpCodes.Newobj, constructor); il.Emit(OpCodes.Stloc, output);
        il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Brfalse, done);
        il.Emit(OpCodes.Br, inputCheck);
        il.Append(inputBody); il.Emit(OpCodes.Ldloc, inputIndex); il.Emit(OpCodes.Callvirt, item); il.Emit(OpCodes.Stloc, data);
        il.Emit(OpCodes.Ldloc, data); il.Emit(OpCodes.Brfalse, next);
        il.Emit(OpCodes.Ldloc, data); il.Emit(OpCodes.Callvirt, isBuy); il.Emit(OpCodes.Brfalse, next);
        il.Emit(OpCodes.Ldc_I4_0); il.Emit(OpCodes.Stloc, outputIndex); il.Emit(OpCodes.Br, outputCheck);
        il.Append(outputBody); il.Emit(OpCodes.Ldloc, outputIndex); il.Emit(OpCodes.Callvirt, item); il.Emit(OpCodes.Callvirt, getIndex);
        il.Emit(OpCodes.Ldloc, data); il.Emit(OpCodes.Callvirt, getIndex); il.Emit(OpCodes.Beq, next);
        il.Emit(OpCodes.Ldloc, outputIndex); il.Emit(OpCodes.Ldc_I4_1); il.Emit(OpCodes.Add); il.Emit(OpCodes.Stloc, outputIndex);
        il.Append(outputCheck); il.Emit(OpCodes.Ldloc, output); il.Emit(OpCodes.Callvirt, count); il.Emit(OpCodes.Blt, outputBody);
        il.Emit(OpCodes.Ldloc, output); il.Emit(OpCodes.Ldloc, data); il.Emit(OpCodes.Callvirt, add);
        il.Append(next); il.Emit(OpCodes.Ldc_I4_1); il.Emit(OpCodes.Add); il.Emit(OpCodes.Stloc, inputIndex);
        il.Append(inputCheck); il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Callvirt, count); il.Emit(OpCodes.Blt, inputBody);
        il.Append(done); il.Emit(OpCodes.Ret);
        Console.WriteLine("Accessory shop lists each purchasable accessory ID once.");
    }

    static void InstallAccessoryShopDuplicates(string clientRoot, bool stageOnly)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        var path = Path.Combine(managed, "Assembly-CSharp.dll");
        var resolver = new DefaultAssemblyResolver(); resolver.AddSearchDirectory(managed);
        using var assembly = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { AssemblyResolver = resolver, InMemory = true });
        PatchAccessoryShopDuplicates(assembly.MainModule);
        var staged = path + ".accessory-list-staged"; assembly.Write(staged);
        if (stageOnly) { Console.WriteLine("Staged " + staged); return; }
        File.Copy(path, path + ".before-accessory-list-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        File.Copy(staged, path, true);
        Console.WriteLine("Installed focused accessory list patch.");
    }
}
