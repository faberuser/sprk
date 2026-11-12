using Mono.Cecil;
using Mono.Cecil.Cil;
namespace DllPatcher;
partial class Program {
    static void PatchEssenceCraftTickets(ModuleDefinition module,ModuleDefinition helper) {
        var popup=module.GetType("NGame2.NUI.NWindow.CraftItemSelectMessagePopup");
        var bridge=helper.GetType("SprkEssenceCraftTickets");
        var indicator=module.GetType("NGame2.NUI.NComponent2.AwakeChallengeItemComponent").Methods.Single(m=>m.Name=="IsCraft");
        if(!indicator.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name==bridge.Name && m.Name=="Indicator")) {
            var first=indicator.Body.Instructions[0];var il=indicator.Body.GetILProcessor();
            var value=new VariableDefinition(module.TypeSystem.Int32);indicator.Body.Variables.Add(value);indicator.Body.InitLocals=true;
            foreach(var instruction in new[]{il.Create(OpCodes.Ldarg_1),il.Create(OpCodes.Ldarg_2),il.Create(OpCodes.Conv_I8),il.Create(OpCodes.Ldarg_3),
                il.Create(OpCodes.Call,module.ImportReference(bridge.Methods.Single(m=>m.Name=="Indicator"))),il.Create(OpCodes.Stloc,value),
                il.Create(OpCodes.Ldloc,value),il.Create(OpCodes.Ldc_I4_M1),il.Create(OpCodes.Beq,first),
                il.Create(OpCodes.Ldloc,value),il.Create(OpCodes.Ldc_I4_1),il.Create(OpCodes.Ceq),il.Create(OpCodes.Ret)})il.InsertBefore(first,instruction);
        }
        foreach(var name in new[]{"SetMinMax","SetMaterial"}) {
            var method=popup.Methods.Single(m=>m.Name==name);var il=method.Body.GetILProcessor();
            foreach(var call in method.Body.Instructions.Where(i=>i.Operand is MethodReference m && m.Name=="GetItemCount" && m.DeclaringType.Name=="UserItemManager").ToArray()) {
                il.InsertBefore(call,il.Create(OpCodes.Ldarg_0));call.OpCode=OpCodes.Call;
                call.Operand=module.ImportReference(bridge.Methods.Single(m=>m.Name=="Available"));
            }
            foreach(var i in method.Body.Instructions.Where(i=>i.OpCode.OperandType==OperandType.ShortInlineBrTarget))
                i.OpCode=typeof(OpCodes).GetFields().Select(f=>f.GetValue(null)).OfType<OpCode>().Single(c=>c.Name==i.OpCode.Name[..^2]);
        }
        var open=popup.Methods.Single(m=>m.Name=="OpenInternal");
        if(!open.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name==bridge.Name && m.Name=="Prefill")) {
            var il=open.Body.GetILProcessor();
            foreach(var ret in open.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=OpCodes.Ldarg_0;var count=il.Create(OpCodes.Ldarg_3);il.InsertAfter(ret,count);
                var call=il.Create(OpCodes.Call,module.ImportReference(bridge.Methods.Single(m=>m.Name=="Prefill")));
                il.InsertAfter(count,call);il.InsertAfter(call,il.Create(OpCodes.Ret));
            }
        }
        foreach(var i in open.Body.Instructions.Where(i=>i.OpCode.OperandType==OperandType.ShortInlineBrTarget))
            i.OpCode=typeof(OpCodes).GetFields().Select(f=>f.GetValue(null)).OfType<OpCode>().Single(c=>c.Name==i.OpCode.Name[..^2]);
        var craft=popup.Methods.Single(m=>m.Name=="OnClickCraft");
        if(!craft.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name==bridge.Name && m.Name=="TryCraft")) {
            var first=craft.Body.Instructions[0];var il=craft.Body.GetILProcessor();
            il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first,il.Create(OpCodes.Ldfld,popup.Fields.Single(f=>f.Name=="_onCraft")));
            il.InsertBefore(first,il.Create(OpCodes.Call,module.ImportReference(bridge.Methods.Single(m=>m.Name=="TryCraft"))));
            il.InsertBefore(first,il.Create(OpCodes.Brfalse,first));il.InsertBefore(first,il.Create(OpCodes.Ret));
        }
        VerifyEssenceCraftTickets(module);
    }
    static void VerifyEssenceCraftTickets(ModuleDefinition module) {
        var indicator=module.GetType("NGame2.NUI.NComponent2.AwakeChallengeItemComponent").Methods.Single(m=>m.Name=="IsCraft");
        if(indicator.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkEssenceCraftTickets" && m.Name=="Indicator")!=1)
            throw new InvalidOperationException("Essence craft indicator hook missing or duplicated");
        var popup=module.GetType("NGame2.NUI.NWindow.CraftItemSelectMessagePopup");
        foreach(var pair in new[]{("SetMinMax","Available"),("SetMaterial","Available"),("OpenInternal","Prefill"),("OnClickCraft","TryCraft")}) {
            var method=popup.Methods.Single(m=>m.Name==pair.Item1);
            int expected=pair.Item1=="OpenInternal"?method.Body.Instructions.Count(i=>i.OpCode==OpCodes.Ret):1;
            if(method.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkEssenceCraftTickets" && m.Name==pair.Item2)!=expected)
                throw new InvalidOperationException("Missing/duplicated essence craft hook: "+pair.Item1);
            // The small stack verifier does not model foreach finally blocks.
            // Validate the replaced call signatures directly for those methods.
            if(method.Body.ExceptionHandlers.Count==0)VerifyArenaResultStack(method);
            if(pair.Item2=="Available") {
                var hook=method.Body.Instructions.Single(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkEssenceCraftTickets" && m.Name=="Available");
                var signature=(MethodReference)hook.Operand;
                if(hook.Previous.OpCode!=OpCodes.Ldarg_0 || signature.HasThis || signature.ReturnType.MetadataType!=MetadataType.Int32 ||
                    !signature.Parameters.Select(p=>p.ParameterType.FullName).SequenceEqual(new[]{"NGame2.NAccount.UserItemManager","System.String",popup.FullName}))
                    throw new InvalidOperationException("Essence material count hook stack signature changed");
            }
        }
        Console.WriteLine("Verified essence-only ticket availability for count/cost/enable state, required quantity prefill and guarded ticket redemption before native crafting.");
    }
}
