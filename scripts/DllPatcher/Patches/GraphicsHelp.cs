using Mono.Cecil;
using Mono.Cecil.Cil;
namespace DllPatcher;
partial class Program {
    static readonly (string Method,string Hook)[] GraphicsHelpHooks={
        ("SetUIRoot","ConfigureGraphicsHelp"),("OnShowTooltip","ShowGraphicsHelp"),("OnHideTooltip","HideGraphicsHelp")};
    static void InstallGraphicsHelp(ModuleDefinition module,TypeDefinition bridge) {
        var type=module.GetType("NGame2.NUI.NComponent2.GameOptionBaseComponent");
        foreach(var pair in GraphicsHelpHooks) {
            var method=type.Methods.Single(m=>m.Name==pair.Method);
            if(method.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.DeclaringType.Name==bridge.Name && r.Name==pair.Hook))continue;
            var il=method.Body.GetILProcessor();var hook=module.ImportReference(bridge.Methods.Single(m=>m.Name==pair.Hook));
            if(pair.Method=="SetUIRoot") {
                foreach(var ret in method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                    ret.OpCode=OpCodes.Ldarg_0;var call=il.Create(OpCodes.Call,hook);il.InsertAfter(ret,call);il.InsertAfter(call,il.Create(OpCodes.Ret));
                }
            } else {
                var first=method.Body.Instructions[0];
                il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));il.InsertBefore(first,il.Create(OpCodes.Call,hook));
                il.InsertBefore(first,il.Create(OpCodes.Brfalse,first));il.InsertBefore(first,il.Create(OpCodes.Ret));
            }
        }
        VerifyGraphicsHelp(module);
    }
    static void VerifyGraphicsHelp(ModuleDefinition module) {
        var type=module.GetType("NGame2.NUI.NComponent2.GameOptionBaseComponent");
        foreach(var pair in GraphicsHelpHooks) {
            var method=type.Methods.Single(m=>m.Name==pair.Method);
            int expected=pair.Method=="SetUIRoot"?method.Body.Instructions.Count(i=>i.OpCode==OpCodes.Ret):1;
            if(method.Body.Instructions.Count(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="SprkGraphicsOptions" && r.Name==pair.Hook)!=expected)
                throw new InvalidOperationException("Missing or duplicated graphics help integration: "+pair.Method);
            VerifyArenaResultStack(method);
        }
    }
}
