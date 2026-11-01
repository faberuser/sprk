using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    const string FriendlyDuelMenuType = "NGame2.NUI.NWindow.InspectUserMenuPopup";

    static void PatchFriendlyDuelMenu(ModuleDefinition module)
    {
        var menu = module.GetType(FriendlyDuelMenuType);
        var open = menu.Methods.Single(m => m.Name == "OpenInternal");
        var request = menu.Methods.Single(m => m.Name == "RequestCanDuel");
        const string marker = "SprkSetDuelButtonVisible";
        if (!menu.Methods.Any(m=>m.Name==marker)) {
            var button=open.Body.Instructions.Single(i=>i.Operand is FieldReference f && f.Name=="DuelButton");
            var enable=open.Body.Instructions.Select(i=>i.Operand).OfType<MethodReference>()
                .First(m=>m.Name=="EnableGameObject" && m.Parameters.Count==3);
            var show=new MethodDefinition(marker,MethodAttributes.Public|MethodAttributes.HideBySig,module.TypeSystem.Void);
            show.Parameters.Add(new ParameterDefinition("isFriend",ParameterAttributes.None,module.TypeSystem.Boolean));
            menu.Methods.Add(show);var code=show.Body.GetILProcessor();
            code.Emit(OpCodes.Ldarg_0);code.Emit(OpCodes.Ldfld,(FieldReference)button.Operand);
            code.Emit(OpCodes.Ldarg_1);code.Emit(OpCodes.Ldc_I4_0);code.Emit(OpCodes.Call,enable);code.Emit(OpCodes.Ret);
            var il=open.Body.GetILProcessor();var before=button.Previous;
            foreach(var instruction in new[]{il.Create(OpCodes.Ldarg_0),il.Create(OpCodes.Ldloc_0),il.Create(OpCodes.Call,show)})
                il.InsertBefore(before,instruction);
        }
        if (!open.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == request.Name))
        {
            // The shipped menu hides DuelButton but never asks the peer whether
            // it can duel. Restore the native handshake, after the target is set
            // and before the final layout. The native callback shows the button.
            var refresh = open.Body.Instructions.Single(i => i.Operand is MethodReference m && m.Name == "RefreshButton");
            var il = open.Body.GetILProcessor();
            il.InsertBefore(refresh.Previous, il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(refresh.Previous, il.Create(OpCodes.Call, request));
        }
        foreach(var method in new[]{open,menu.Methods.Single(m=>m.Name=="<RequestCanDuel>b__25_0")}) {
            var enable=method.Body.Instructions.Single(i=>i.Operand is MethodReference m && m.Name=="EnableButton");
            enable.Previous.Previous.OpCode=OpCodes.Ldc_I4_1;
        }
        VerifyFriendlyDuelMenu(module);
    }

    static void VerifyFriendlyDuelMenu(ModuleDefinition module)
    {
        var menu = module.GetType(FriendlyDuelMenuType);
        var open = menu.Methods.Single(m => m.Name == "OpenInternal");
        bool Calls(MethodDefinition method, string name) => method.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == name);
        var hook = open.Body.Instructions.Single(i => i.Operand is MethodReference m && m.Name == "RequestCanDuel");
        var target = open.Body.Instructions.Single(i => i.OpCode == OpCodes.Stfld && i.Operand is FieldReference f && f.Name == "_inspectUser");
        var refresh = open.Body.Instructions.Single(i => i.Operand is MethodReference m && m.Name == "RefreshButton");
        var visible=open.Body.Instructions.Single(i=>i.Operand is MethodReference m && m.Name=="SprkSetDuelButtonVisible");
        var show=menu.Methods.Single(m=>m.Name=="SprkSetDuelButtonVisible");
        if (visible.Previous.OpCode!=OpCodes.Ldloc_0 || !Calls(show,"EnableGameObject") ||
            !open.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="IsFriend"))
            throw new InvalidOperationException("The duel GameObject must be explicitly shown for friends.");
        if (open.Body.Instructions.IndexOf(target) >= open.Body.Instructions.IndexOf(hook) ||
            open.Body.Instructions.IndexOf(hook) >= open.Body.Instructions.IndexOf(refresh) || hook.Previous.OpCode != OpCodes.Ldarg_0)
            throw new InvalidOperationException("Duel availability must be requested after assigning the selected friend and before layout.");
        var request = menu.Methods.Single(m => m.Name == "RequestCanDuel");
        var callback = menu.Methods.Single(m => m.Name == "<RequestCanDuel>b__25_0");
        var click = menu.Methods.Single(m => m.Name == "OnClickButton");
        if (!Calls(request, "IsValidParty") || !Calls(request, "CanDuel") || !Calls(request, "SendCanDuel") ||
            !Calls(callback, "get_CanDuel") || !Calls(callback, "EnableButton") || !Calls(callback, "RefreshButton") ||
            !Calls(click, "RequestDuel") || !callback.Body.Instructions.Any(i => i.Operand is FieldReference f && f.Name == "DuelButton"))
            throw new InvalidOperationException("Friendly duels must retain native party/eligibility checks, peer response and invitation action.");
        foreach (var method in new[] { open, request, callback, click, show }) VerifyArenaResultStack(method);
    }

    static void InstallFriendlyDuelMenu(string root, bool verifyOnly)
    {
        var dll = Path.GetFullPath(Path.Combine(root, "King's Raid_Data", "Managed", "Assembly-CSharp.dll"));
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(dll)!);
        using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver }))
        {
            if (verifyOnly) VerifyFriendlyDuelMenu(assembly.MainModule);
            else
            {
                PatchFriendlyDuelMenu(assembly.MainModule);
                PatchFriendlyDuelMenu(assembly.MainModule);
                var backup = Path.Combine("target", "friendly-duel-menu-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
                Directory.CreateDirectory(backup);
                File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
                assembly.Write(dll + ".friendly-duel-menu-staged");
            }
        }
        resolver.Dispose();
        if (!verifyOnly) File.Move(dll + ".friendly-duel-menu-staged", dll, true);
        Console.WriteLine("Verified friend menu duel availability handshake, native invitation flow and IL stacks.");
    }
}
