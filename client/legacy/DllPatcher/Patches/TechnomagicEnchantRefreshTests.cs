using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;
using System.Runtime.CompilerServices;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace DllPatcher;
partial class Program {
    static void TestTechnomagicEnchantRefresh(AssemblyDefinition source,string managed) {
        using var bytes=new MemoryStream();source.Write(bytes);bytes.Position=0;
        using var fixture=AssemblyDefinition.ReadAssembly(bytes,new ReaderParameters{AssemblyResolver=source.MainModule.AssemblyResolver});
        var game=fixture.MainModule;var window=game.GetType("NGame2.NUI.NWindow.ValanceEnchantWindow");
        var info=game.GetType("NShared.EquipItemInfo");var pending=game.GetType("NShared.ValanceEnchantOptionInfo");
        FieldDefinition Field(string name,TypeReference type) {
            var f=new FieldDefinition(name,Mono.Cecil.FieldAttributes.Public|Mono.Cecil.FieldAttributes.Static,type);window.Fields.Add(f);return f;
        }
        var current=Field("SPRKCurrent",info);var observed=Field("SPRKObserved",info);var choice=Field("SPRKChoice",pending);var slot=Field("SPRKSlot",game.TypeSystem.Int32);
        void NoOp(MethodDefinition method) {method.Body=new MethodBody(method);method.Body.GetILProcessor().Emit(OpCodes.Ret);}
        void Capture(MethodDefinition method,FieldDefinition field) {
            method.Body=new MethodBody(method);var il=method.Body.GetILProcessor();il.Emit(OpCodes.Ldarg_1);il.Emit(OpCodes.Stsfld,field);il.Emit(OpCodes.Ret);
        }
        var component=game.GetType("ValanceEnchantOptionComponent");
        Capture(component.Methods.Single(m=>m.Name=="ShowCurrentOptionList"),observed);
        Capture(component.Methods.Single(m=>m.Name=="ShowNewOptionList"),choice);
        foreach(var name in new[]{"SetPanelState","ShowConfirmButton"})NoOp(component.Methods.Single(m=>m.Name==name));
        NoOp(window.Methods.Single(m=>m.Name=="Refresh"));
        NoOp(game.GetType("NGame2.NUI.NWindow.NValance.ValanceItemListPanel").Methods.Single(m=>m.Name=="DisableAllItemSelect"));
        var helper=window.Methods.Single(m=>m.Name==TmEnchantTarget);
        var getCall=helper.Body.Instructions.Single(i=>i.Operand is MethodReference m && m.Name=="Get");
        var lookup=new MethodDefinition("SPRKLookup",Mono.Cecil.MethodAttributes.Private|Mono.Cecil.MethodAttributes.Static,info);
        lookup.Parameters.Add(new ParameterDefinition(game.GetType("NGame2.NAccount.UserEquipItemManager")));
        lookup.Parameters.Add(new ParameterDefinition(game.TypeSystem.Int32));window.Methods.Add(lookup);
        var il=lookup.Body.GetILProcessor();il.Emit(OpCodes.Ldarg_1);il.Emit(OpCodes.Stsfld,slot);il.Emit(OpCodes.Ldsfld,current);il.Emit(OpCodes.Ret);
        getCall.OpCode=OpCodes.Call;getCall.Operand=lookup;
        // Keep the native response with only our lookup reverted. This runs
        // the pre-fix failure and the corrected handler against identical data.
        var response=window.Methods.Single(m=>m.Name=="OnEnchantResponse");
        var before=new MethodDefinition("SPRKBeforeFixResponse",Mono.Cecil.MethodAttributes.Public,game.TypeSystem.Void);
        foreach(var parameter in response.Parameters)before.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
        before.Body=new MethodBody(before){InitLocals=response.Body.InitLocals};window.Methods.Add(before);
        foreach(var variable in response.Body.Variables)before.Body.Variables.Add(new VariableDefinition(variable.VariableType));
        var copies=response.Body.Instructions.ToDictionary(i=>i,i=>Instruction.Create(OpCodes.Nop));
        foreach(var original in response.Body.Instructions) {
            var copy=copies[original];copy.OpCode=original.OpCode;copy.Operand=original.Operand;
            if(original.Operand is Instruction branch)copy.Operand=copies[branch];
            if(original.Operand is VariableDefinition variable)copy.Operand=before.Body.Variables[response.Body.Variables.IndexOf(variable)];
            if(original.Operand is ParameterDefinition parameter)copy.Operand=before.Parameters[response.Parameters.IndexOf(parameter)];
            if(original.Operand is MethodReference method && method.Name==TmEnchantTarget) {
                copy.OpCode=OpCodes.Callvirt;copy.Operand=game.GetType("NGame2.NUI.NComponent2.ItemIconComponent").Methods.Single(m=>m.Name=="get_EquipItemInfo");
            }
            before.Body.Instructions.Add(copy);
        }
        var account=game.GetType("NGame2.NAccount.AccountManager");
        var equipGetter=account.Methods.Single(m=>m.Name=="get_EquipItemManager");equipGetter.Body=new MethodBody(equipGetter);
        equipGetter.Body.GetILProcessor().Emit(OpCodes.Ldnull);equipGetter.Body.GetILProcessor().Emit(OpCodes.Ret);
        var userGetter=account.Methods.Single(m=>m.Name=="get_UserManager");var user=Field("SPRKUserManager",userGetter.ReturnType);
        userGetter.Body=new MethodBody(userGetter);userGetter.Body.GetILProcessor().Emit(OpCodes.Ldsfld,user);userGetter.Body.GetILProcessor().Emit(OpCodes.Ret);
        NoOp(game.GetType("NGame2.NAccount.UserManager").Methods.Single(m=>m.Name=="SetValanceEnchant"));
        NoOp(game.GetType("NVespa.NSingleton.GlobalAutoStatic`1").Methods.Single(m=>m.Name=="CreateInstance"));
        fixture.Name.Name="SPRK.TmEnchant.Fixture."+Guid.NewGuid().ToString("N");
        using var output=new MemoryStream();fixture.Write(output);
        ResolveEventHandler resolve=(_,e)=>{var path=Path.Combine(managed,new AssemblyName(e.Name).Name+".dll");return File.Exists(path)?Assembly.LoadFrom(path):null;};
        AppDomain.CurrentDomain.AssemblyResolve+=resolve;
        try {
            var native=Assembly.Load(output.ToArray());var flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
            var type=native.GetType(window.FullName)!;var infoType=native.GetType(info.FullName)!;
            object Fresh(string name)=>RuntimeHelpers.GetUninitializedObject(native.GetType(name)!);
            var accountType=native.GetType(account.FullName)!;
            native.GetType("NVespa.NSingleton.GlobalAutoStatic`1")!.MakeGenericType(accountType)
                .GetField("_instance",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static)!.SetValue(null,Fresh(account.FullName));
            type.GetField(user.Name)!.SetValue(null,Fresh(userGetter.ReturnType.FullName));
            var ui=Fresh(window.FullName);var target=Fresh("NGame2.NUI.NComponent2.ItemIconComponent");
            var old=Activator.CreateInstance(infoType)!;infoType.GetProperty("SlotIndex")!.SetValue(old,123);
            target.GetType().GetProperty("EquipItemInfo")!.SetValue(target,old);
            type.BaseType!.GetField("TargetComponent",flags)!.SetValue(ui,target);
            type.GetField("currentOptionComponent")!.SetValue(ui,Fresh(component.FullName));
            type.GetField("newOptionComponent")!.SetValue(ui,Fresh(component.FullName));
            type.BaseType.GetField("ItemListPanel")!.SetValue(ui,Fresh("NGame2.NUI.NWindow.NValance.ValanceItemListPanel"));
            var newInfo=Activator.CreateInstance(native.GetType(pending.FullName)!)!;
            var first=Activator.CreateInstance(infoType)!;infoType.GetProperty("SlotIndex")!.SetValue(first,123);
            infoType.GetProperty("EnchantOptionIndex1")!.SetValue(first,101);type.GetField(current.Name)!.SetValue(null,first);
            type.GetField("EnchantInProgress")!.SetValue(null,false);
            type.GetMethod("SPRKBeforeFixResponse")!.Invoke(ui,new[]{newInfo});
            if(!ReferenceEquals(type.GetField(observed.Name)!.GetValue(null),old))throw new InvalidOperationException("Stale-reference reproduction did not exercise the original failure");
            for(int i=1;i<=5;i++) {
                var updated=Activator.CreateInstance(infoType)!;infoType.GetProperty("SlotIndex")!.SetValue(updated,123);
                infoType.GetProperty("EnchantOptionIndex1")!.SetValue(updated,100+i);
                type.GetField(current.Name)!.SetValue(null,updated);type.GetField("EnchantInProgress")!.SetValue(null,false);
                type.GetMethod("OnEnchantResponse")!.Invoke(ui,new[]{newInfo});
                if(!ReferenceEquals(type.GetField(observed.Name)!.GetValue(null),updated) || (int)type.GetField(slot.Name)!.GetValue(null)!!=123)
                    throw new InvalidOperationException("First TM enchant displayed a stale target instead of current slot data");
                type.GetField(observed.Name)!.SetValue(null,null);type.GetMethod("OnEnchantConfirmResponse")!.Invoke(ui,null);
                if(!ReferenceEquals(type.GetField(observed.Name)!.GetValue(null),updated))throw new InvalidOperationException("Confirm displayed stale enchant options");
                type.GetField("EnchantInProgress")!.SetValue(null,true);type.GetField(observed.Name)!.SetValue(null,null);
                type.GetMethod("OnEnchantResponse")!.Invoke(ui,new[]{newInfo});
                if(!ReferenceEquals(type.GetField(choice.Name)!.GetValue(null),newInfo) || type.GetField(observed.Name)!.GetValue(null)!=null)
                    throw new InvalidOperationException("TM reroll must retain its native pending-choice path");
            }
            type.GetField(current.Name)!.SetValue(null,null);
            var get=type.GetMethod(TmEnchantTarget,BindingFlags.NonPublic|BindingFlags.Static)!;
            if(!ReferenceEquals(get.Invoke(null,new[]{target}),old))throw new InvalidOperationException("Missing canonical gear lost the selected target");
            target.GetType().GetProperty("EquipItemInfo")!.SetValue(target,null);
            if(get.Invoke(null,new[]{target})!=null)throw new InvalidOperationException("Empty target must remain empty");
            Console.WriteLine("PASS: actual native first-enchant and confirm handlers render current slot data through five stale-reference cycles; reroll choices, missing gear and empty targets preserved.");
        } finally {AppDomain.CurrentDomain.AssemblyResolve-=resolve;}
    }
}
