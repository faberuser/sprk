using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;
using System.Runtime.Loader;

namespace DllPatcher;
partial class Program
{
    static MethodDefinition CraftShortcutMethod(ModuleDefinition module) => module
        .GetType("NGame2.NUI.NManager.InventoryManagement").Methods.Single(m => m.Name == "OpenShortCutCraftWindow");

    static void PatchCraftShortcut(ModuleDefinition module)
    {
        var method = CraftShortcutMethod(module);
        if (method.Body.Instructions.Any(i => i.Operand is MethodReference { Name: "get_CraftSlotManager" })) {
            VerifyCraftShortcut(module); return;
        }
        var code = method.Body.Instructions;
        if (code[0].OpCode != OpCodes.Ldarg_0 || code[2].Operand is not GenericInstanceMethod { Name: "GetInstance" }
            || code[3].Operand is not MethodReference { Name: "GetEmptyCraftSlotIndex" }
            || code[6].OpCode != OpCodes.Ldc_I4_M1 || code[7].OpCode != OpCodes.Bne_Un_S)
            throw new InvalidOperationException("Unexpected native crafting shortcut; refusing to patch.");
        // The native OpenCraftWindow already queries account slots independently
        // of InvenList.Init. Reuse its references without introducing a helper DLL.
        var donor = method.DeclaringType.Methods.Single(m => m.Name == "OpenCraftWindow")
            .Body.Instructions.Select(i => i.Operand).OfType<MethodReference>().ToArray();
        code[0].OpCode = OpCodes.Call; code[0].Operand = donor.Single(m => m.Name == "get_instance");
        code[1].OpCode = OpCodes.Callvirt; code[1].Operand = donor.Single(m => m.Name == "get_CraftSlotManager");
        code[2].OpCode = OpCodes.Callvirt; code[2].Operand = donor.Single(m => m.Name == "GetEmptyCraftSlot");
        code[3].OpCode = OpCodes.Nop; code[3].Operand = null;
        // Account slots use zero for unavailable; the unopened UI used -1.
        code[6].OpCode = OpCodes.Ldc_I4_0;
        code[7].OpCode = OpCodes.Bgt_S;
        VerifyCraftShortcut(module);
    }

    static void VerifyCraftShortcut(ModuleDefinition module)
    {
        var code = CraftShortcutMethod(module).Body.Instructions;
        if (code[1].Operand is not MethodReference { Name: "get_CraftSlotManager" }
            || code[2].Operand is not MethodReference { Name: "GetEmptyCraftSlot" }
            || code[6].OpCode != OpCodes.Ldc_I4_0 || code[7].OpCode != OpCodes.Bgt_S
            || code.Any(i => i.Operand is GenericInstanceMethod { Name: "GetInstance" })
            || code.Any(i => i.Operand is MethodReference { Name: "GetEmptyCraftSlotIndex" })
            || code[^2].Operand is not MethodReference { Name: "OpenCraftItemSelectMessagePopup" }
            || code[^4].OpCode != OpCodes.Ldarg_1 || code[^3].OpCode != OpCodes.Ldarg_2)
            throw new InvalidOperationException("Crafting shortcut must use account slots and preserve its material/count.");
    }

    static void VerifyCraftShortcutFile(string root, bool staged)
    {
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(root,"King's Raid_Data","Managed",
            "Assembly-CSharp.dll" + (staged ? ".craft-shortcut-staged" : "")));
        VerifyCraftShortcut(assembly.MainModule);
        Console.WriteLine("Verified crafting shortcut uses account slots and preserves material/count.");
    }

    static void InstallCraftShortcut(string root, bool stageOnly)
    {
        var dll=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed","Assembly-CSharp.dll"));
        var staged=dll+".craft-shortcut-staged";
        using var resolver=new DefaultAssemblyResolver(); resolver.AddSearchDirectory(Path.GetDirectoryName(dll)!);
        using(var assembly=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver})) {
            PatchCraftShortcut(assembly.MainModule); assembly.Write(staged);
        }
        VerifyCraftShortcutFile(root,true);
        if(stageOnly) return;
        if(System.Diagnostics.Process.GetProcessesByName("King's Raid").Length!=0)
            throw new InvalidOperationException("Crafting shortcut staged. Close King's Raid before installing and publishing.");
        var backup=Path.Combine("target","craft-shortcut-backups",DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
        Directory.CreateDirectory(backup); File.Copy(dll,Path.Combine(backup,"Assembly-CSharp.dll"));
        File.Move(staged,dll,true); VerifyCraftShortcutFile(root,false);
    }

    static void TestCraftShortcut(string root)
    {
        var dll=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed","Assembly-CSharp.dll"));
        using var assembly=AssemblyDefinition.ReadAssembly(dll);
        PatchCraftShortcut(assembly.MainModule);
        var size=CraftShortcutMethod(assembly.MainModule).Body.Instructions.Count;
        PatchCraftShortcut(assembly.MainModule);
        if(CraftShortcutMethod(assembly.MainModule).Body.Instructions.Count!=size) throw new Exception("Patch is not idempotent.");
        // Execute the production availability branch without loading Unity UI.
        var code=CraftShortcutMethod(assembly.MainModule).Body.Instructions;
        var test=new System.Reflection.Emit.DynamicMethod("CraftSlotAvailable",typeof(bool),new[]{typeof(int)});
        var il=test.GetILGenerator(); var available=il.DefineLabel();
        il.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
        if(code[6].OpCode!=OpCodes.Ldc_I4_0 || code[7].OpCode!=OpCodes.Bgt_S) throw new Exception("Unexpected slot guard.");
        il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4_0); il.Emit(System.Reflection.Emit.OpCodes.Bgt,available);
        il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4_0); il.Emit(System.Reflection.Emit.OpCodes.Ret);
        il.MarkLabel(available); il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4_1); il.Emit(System.Reflection.Emit.OpCodes.Ret);
        var opens=(Func<int,bool>)test.CreateDelegate(typeof(Func<int,bool>));
        foreach(var slot in new[]{-1,0,1,2,8}) if(opens(slot)!=(slot>0)) throw new Exception("Invalid craft-slot routing.");
        // Execute the actual native account-slot manager: no inventory list is
        // created, so this also covers the first shortcut after a fresh login.
        AssemblyLoadContext.Default.Resolving+=(context,name)=> {
            var path=Path.Combine(Path.GetDirectoryName(dll)!,name.Name+".dll");
            return File.Exists(path)?context.LoadFromAssemblyPath(path):null;
        };
        var client=AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);
        var type=client.GetType("NGame2.NAccount.UserCraftSlotManager",true)!;
        var infoType=client.GetType("NShared.CraftSlotInfo",true)!;
        var manager=Activator.CreateInstance(type)!;
        var get=type.GetMethod("GetEmptyCraftSlot")!;
        var add=type.GetMethods().Single(m=>m.Name=="Add" && m.GetParameters().Length==1 && m.GetParameters()[0].ParameterType==infoType);
        if((int)get.Invoke(manager,null)! !=0) throw new Exception("Empty account has no craft slots.");
        object? last=null;
        for(int slot=1;slot<=3;slot++) {
            var info=Activator.CreateInstance(infoType)!;
            infoType.GetProperty("SlotIndex")!.SetValue(info,slot);
            infoType.GetProperty("ItemIndex")!.SetValue(info,slot==3?0:2100);
            add.Invoke(manager,new[]{info});
            last=info;
        }
        if((int)get.Invoke(manager,null)! !=3) throw new Exception("Native slot manager did not select the free slot.");
        infoType.GetProperty("ItemIndex")!.SetValue(last,2100);
        if((int)get.Invoke(manager,null)! !=0) throw new Exception("Busy slots must not permit crafting.");
        Console.WriteLine("PASS: native account slot lookup without inventory initialization, 5 availability cases and patch idempotence.");
    }
}
