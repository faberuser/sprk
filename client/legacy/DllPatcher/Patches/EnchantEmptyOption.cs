using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DllPatcher;
partial class Program {
    const string EmptyEnchantMethod="SprkOpenPreviousEnchantOption";
    static void PatchEnchantEmptyOption(ModuleDefinition game) {
        var scene=game.GetType("NGame2.NUI.NWindow.EquipEnchantOptionScene");
        var gauge=game.GetType("NGame2.NUI.NComponent2.NStat.StatGaugeInfo");
        var open=gauge.Methods.Single(m=>m.Name=="Open" && m.Parameters.Count==3);
        var wrapper=scene.Methods.SingleOrDefault(m=>m.Name==EmptyEnchantMethod);
        if(wrapper==null) {
            wrapper=new MethodDefinition(EmptyEnchantMethod,Mono.Cecil.MethodAttributes.Private|Mono.Cecil.MethodAttributes.Static,game.TypeSystem.Void);
            wrapper.Parameters.Add(new ParameterDefinition("gauge",Mono.Cecil.ParameterAttributes.None,gauge));
            foreach(var p in open.Parameters)wrapper.Parameters.Add(new ParameterDefinition(p.Name,Mono.Cecil.ParameterAttributes.None,p.ParameterType));
            scene.Methods.Add(wrapper);
            var localization=game.GetType("NShared.NLocalization.EquipEnchant");
            var empty=Convert.ToInt32(localization.Fields.Single(f=>f.Name=="EMPTY_ENCHANT_OPTION").Constant);
            var getString=game.GetType("NVespa.NGlobal.LocalizationManager").Methods.Single(m=>m.Name=="GetString" && m.Parameters.Count==2 && m.Parameters[0].ParameterType.FullName=="System.Enum");
            var il=wrapper.Body.GetILProcessor();var normal=il.Create(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);il.Emit(OpCodes.Unbox_Any,game.GetType("NShared.OperationType"));il.Emit(OpCodes.Brtrue,normal);
            il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldc_I4,empty);il.Emit(OpCodes.Box,localization);
            il.Emit(OpCodes.Ldc_I4_0);il.Emit(OpCodes.Newarr,game.TypeSystem.Object);il.Emit(OpCodes.Call,getString);
            il.Emit(OpCodes.Callvirt,gauge.Methods.Single(m=>m.Name=="OpenEmpty"));il.Emit(OpCodes.Ret);
            il.Append(normal);il.Emit(OpCodes.Ldarg_1);il.Emit(OpCodes.Ldarg_2);il.Emit(OpCodes.Ldarg_3);il.Emit(OpCodes.Callvirt,open);il.Emit(OpCodes.Ret);
        }
        var show=scene.Methods.Single(m=>m.Name=="ShowAwakenResult");
        if(!show.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.Name==EmptyEnchantMethod)) {
            // The first gauge is the previous option; the second is the new roll.
            var previous=show.Body.Instructions.First(i=>i.Operand is MethodReference r && r.DeclaringType==gauge && r.Name=="Open");
            previous.OpCode=OpCodes.Call;previous.Operand=wrapper;
        }
        VerifyEnchantEmptyOption(game);
    }
    static void VerifyEnchantEmptyOption(ModuleDefinition game) {
        var scene=game.GetType("NGame2.NUI.NWindow.EquipEnchantOptionScene");
        var show=scene.Methods.Single(m=>m.Name=="ShowAwakenResult");
        var wrapper=scene.Methods.Single(m=>m.Name==EmptyEnchantMethod);
        if(show.Body.Instructions.Count(i=>i.Operand is MethodReference r && r.Name==EmptyEnchantMethod)!=1 ||
           show.Body.Instructions.Count(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="StatGaugeInfo" && r.Name=="Open")!=1)
            throw new Exception("Enchant empty state must replace only the previous-option gauge");
        foreach(var method in new[]{show,wrapper})VerifyArenaResultStack(method);
        var calls=wrapper.Body.Instructions.Select(i=>i.Operand).OfType<MethodReference>().Select(r=>r.Name).ToArray();
        if(!calls.Contains("OpenEmpty") || !calls.Contains("GetString") || !calls.Contains("Open"))throw new Exception("Missing native localized empty enchant display");
        Console.WriteLine("Verified localized first-enchant empty state and preserved new/existing option gauges.");
    }
    static void TestEnchantEmptyOption(string managed,AssemblyDefinition source) {
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var bytes=new MemoryStream();source.Write(bytes);bytes.Position=0;
        using var fixture=AssemblyDefinition.ReadAssembly(bytes,new ReaderParameters {AssemblyResolver=resolver});
        var game=fixture.MainModule;var scene=game.GetType("NGame2.NUI.NWindow.EquipEnchantOptionScene");
        var marker=new FieldDefinition("SPRKTestGauge",Mono.Cecil.FieldAttributes.Public|Mono.Cecil.FieldAttributes.Static,game.TypeSystem.Int32);scene.Fields.Add(marker);
        var gauge=game.GetType("NGame2.NUI.NComponent2.NStat.StatGaugeInfo");
        foreach(var entry in new[]{("Open",3,1),("OpenEmpty",1,2)}) {
            var method=gauge.Methods.Single(m=>m.Name==entry.Item1 && m.Parameters.Count==entry.Item2);
            method.Body=new Mono.Cecil.Cil.MethodBody(method);var il=method.Body.GetILProcessor();
            il.Emit(OpCodes.Ldc_I4,entry.Item3);il.Emit(OpCodes.Stsfld,marker);il.Emit(OpCodes.Ret);
        }
        var localization=game.GetType("NVespa.NGlobal.LocalizationManager").Methods.Single(m=>m.Name=="GetString" && m.Parameters.Count==2 && m.Parameters[0].ParameterType.FullName=="System.Enum");
        localization.Body=new Mono.Cecil.Cil.MethodBody(localization);localization.Body.Instructions.Add(Instruction.Create(OpCodes.Ldstr,"Empty enchantment"));localization.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        fixture.Name.Name="SPRK.EnchantEmpty.Fixture."+Guid.NewGuid().ToString("N");
        using var output=new MemoryStream();fixture.Write(output);
        ResolveEventHandler resolve=(_,e)=>{var path=Path.Combine(managed,new AssemblyName(e.Name).Name+".dll");return File.Exists(path)?Assembly.LoadFrom(path):null;};
        AppDomain.CurrentDomain.AssemblyResolve+=resolve;
        try {
            var native=Assembly.Load(output.ToArray());var type=native.GetType(scene.FullName)!;
            var method=type.GetMethod(EmptyEnchantMethod,BindingFlags.NonPublic|BindingFlags.Static)!;
            var widget=RuntimeHelpers.GetUninitializedObject(native.GetType(gauge.FullName)!);
            var operation=native.GetType("NShared.OperationType")!;
            foreach(var (stat,expected) in new[]{(0,2),(1,1),(0,2),(2,1)}) {
                method.Invoke(null,new[]{widget,Enum.ToObject(operation,stat),(object)80,(object)140});
                if((int)type.GetField(marker.Name)!.GetValue(null)!!=expected)throw new Exception("Empty/existing enchant gauge dispatch failed");
            }
        } finally {AppDomain.CurrentDomain.AssemblyResolve-=resolve;}
        Console.WriteLine("PASS: actual patched enchant display dispatch handles empty, existing and reused comparison states.");
    }
    static void InstallEnchantEmptyOption(string root,bool stageOnly,bool verifyOnly=false) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));var path=Path.Combine(managed,"Assembly-CSharp.dll");
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(path,new ReaderParameters {InMemory=true,AssemblyResolver=resolver});
        if(verifyOnly){VerifyEnchantEmptyOption(game.MainModule);TestEnchantEmptyOption(managed,game);return;}
        PatchEnchantEmptyOption(game.MainModule);TestEnchantEmptyOption(managed,game);
        var before=game.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).Sum(m=>m.Body.Instructions.Count);
        PatchEnchantEmptyOption(game.MainModule);
        if(before!=game.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).Sum(m=>m.Body.Instructions.Count))throw new Exception("Enchant empty-state patch is not idempotent");
        var staged=path+".enchant-empty-staged";game.Write(staged);if(stageOnly)return;
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
        File.Copy(path,path+".before-enchant-empty-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff"));File.Move(staged,path,true);
        Console.WriteLine("Installed native first-enchant empty display. Restart the client.");
    }
}
