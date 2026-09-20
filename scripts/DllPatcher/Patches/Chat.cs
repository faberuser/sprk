using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static void PatchChatBackground(ModuleDefinition module)
    {
        var window = module.Types.Single(t => t.FullName == "NGame2.NUI.NWindow.NChatting.ChattingWindow");
        const string helperName = "SprkFixChatBackground";
        if (window.Methods.Any(m => m.Name == helperName)) return;
        var references = module.Types.SelectMany(t => t.Methods).Where(m => m.HasBody)
            .SelectMany(m => m.Body.Instructions).Select(i => i.Operand).OfType<MethodReference>().ToArray();
        var transform = references.First(m => m.DeclaringType.FullName == "UnityEngine.Component" && m.Name == "get_transform");
        var find = references.First(m => m.DeclaringType.FullName == "UnityEngine.Transform" && m.Name == "Find" && m.Parameters.Count == 1);
        var sprite = module.Types.Single(t => t.Name == "UISprite");
        var getSprite = references.OfType<GenericInstanceMethod>().First(m => m.Name == "GetComponent"
            && m.DeclaringType.FullName == "UnityEngine.Component" && m.GenericArguments[0].FullName == sprite.FullName);
        var colorCtor = references.First(m => m.DeclaringType.FullName == "UnityEngine.Color" && m.Name == ".ctor" && m.Parameters.Count == 4);
        var widget = module.Types.Single(t => t.Name == "UIWidget");
        var helper = new MethodDefinition(helperName, MethodAttributes.Private | MethodAttributes.HideBySig, module.TypeSystem.Void);
        window.Methods.Add(helper);
        var il = helper.Body.GetILProcessor();
        var foundTransform = il.Create(OpCodes.Callvirt, getSprite);
        var foundSprite = il.Create(OpCodes.Dup);
        il.Append(il.Create(OpCodes.Ldarg_0));
        il.Append(il.Create(OpCodes.Call, transform));
        il.Append(il.Create(OpCodes.Ldstr, "Panel/Root/Sprite_Frame_a"));
        il.Append(il.Create(OpCodes.Callvirt, find));
        il.Append(il.Create(OpCodes.Dup));
        il.Append(il.Create(OpCodes.Brtrue, foundTransform));
        il.Append(il.Create(OpCodes.Pop));
        il.Append(il.Create(OpCodes.Ret));
        il.Append(foundTransform);
        il.Append(il.Create(OpCodes.Dup));
        il.Append(il.Create(OpCodes.Brtrue, foundSprite));
        il.Append(il.Create(OpCodes.Pop));
        il.Append(il.Create(OpCodes.Ret));
        il.Append(foundSprite);
        il.Append(il.Create(OpCodes.Ldstr, "blank_white"));
        il.Append(il.Create(OpCodes.Callvirt, sprite.Methods.Single(m => m.Name == "set_spriteName")));
        // The existing background's bottom highlight stretches into the input row.
        // Keep the separate gold frame and replace only its inner fill.
        foreach (float value in new[] { 0.965f, 0.937f, 0.875f, 1f }) il.Append(il.Create(OpCodes.Ldc_R4, value));
        il.Append(il.Create(OpCodes.Newobj, colorCtor));
        il.Append(il.Create(OpCodes.Callvirt, widget.Methods.Single(m => m.Name == "set_color")));
        il.Append(il.Create(OpCodes.Ret));
        var init = window.Methods.Single(m => m.Name == "Init");
        var initIl = init.Body.GetILProcessor();
        foreach (var ret in init.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
        {
            ret.OpCode = OpCodes.Ldarg_0;
            initIl.InsertAfter(ret, initIl.Create(OpCodes.Call, helper));
            initIl.InsertAfter(ret.Next, initIl.Create(OpCodes.Ret));
        }
        Console.WriteLine("Patched chat background to remove the stretched bottom highlight");
    }

    static void PatchChatSession(ModuleDefinition module)
    {
        // Native LoginReq lacks SessionKey, which the server requires to
        // authenticate the socket. Add it at serialization time from the
        // same requester that owns the authenticated HTTP session.
        var encoder = module.Types.Single(t => t.Name == "JM_NShared_NMessage_LoginReq")
            .Methods.Single(m => m.Name == "EncodeJsonObject");
        if (encoder.Body.Instructions.Any(i => i.OpCode == OpCodes.Ldstr && (string?)i.Operand == "SessionKey"))
            return;
        var requester = module.Types.Single(t => t.FullName == "NVespa.NWeb.WebServiceRequester");
        var manager = module.Types.Single(t => t.FullName == "NVespa.NGlobal.WebServiceManager");
        var instance = manager.Methods.Single(m => m.Name == ".ctor").Body.Instructions
            .Select(i => i.Operand).OfType<MethodReference>()
            .Single(m => m.Name == "get_instance" && m.DeclaringType is GenericInstanceType g
                && g.GenericArguments[0].FullName == requester.FullName);
        var add = encoder.Body.Instructions.Select(i => i.Operand).OfType<MethodReference>()
            .First(m => m.Name == "Add" && m.DeclaringType.Name.StartsWith("Dictionary"));
        var il = encoder.Body.GetILProcessor();
        var ret = encoder.Body.Instructions.Last(i => i.OpCode == OpCodes.Ret);
        ret.OpCode = OpCodes.Dup; // Preserve the dictionary return value.
        il.Append(il.Create(OpCodes.Ldstr, "SessionKey"));
        il.Append(il.Create(OpCodes.Call, instance));
        il.Append(il.Create(OpCodes.Callvirt, requester.Methods.Single(m => m.Name == "get_SessionKey")));
        il.Append(il.Create(OpCodes.Callvirt, add));
        il.Append(il.Create(OpCodes.Ret));
        Console.WriteLine("Patched chat login to include the authenticated game session");
    }
}
