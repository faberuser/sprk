using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
namespace DllPatcher;
partial class Program {
    static void InstallAccounts(string clientRoot, bool stageOnly, string? sourcePath = null) {
        var managed=Path.GetFullPath(Path.Combine(clientRoot,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");
        using var resource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.Accounts.cs.txt")!;
        using var reader=new StreamReader(resource);
        var references=Directory.GetFiles(managed,"*.dll").Where(p=>Path.GetFileName(p)!="SprkAccounts.dll").Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("SprkAccounts",new[]{CSharpSyntaxTree.ParseText(reader.ReadToEnd())},references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release));
        using var bytes=new MemoryStream();var result=compilation.Emit(bytes);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics));
        bytes.Position=0;using var helper=AssemblyDefinition.ReadAssembly(bytes);
        var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var assembly=AssemblyDefinition.ReadAssembly(sourcePath ?? dll,new ReaderParameters{AssemblyResolver=resolver});
        var module=assembly.MainModule;
        PatchLegacyHeroLevels(module);
        PatchDirectAccessorySales(module);
        var bridge=helper.MainModule.Types.Single(t=>t.Name=="SprkAccounts");
        MethodReference Bridge(string name)=>module.ImportReference(bridge.Methods.Single(m=>m.Name==name));
        void After(string type,string method,string helperName,bool passThis=true) {
            var target=module.GetType(type).Methods.Single(m=>m.Name==method);
            if(target.Body.Instructions.Any(i=>i.Operand is MethodReference call && call.DeclaringType.Name=="SprkAccounts" && call.Name==helperName))return;
            var processor=target.Body.GetILProcessor();
            foreach(var ret in target.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=passThis?OpCodes.Ldarg_0:OpCodes.Nop;ret.Operand=null;
                var call=Instruction.Create(OpCodes.Call,Bridge(helperName));
                processor.InsertAfter(ret,call);processor.InsertAfter(call,Instruction.Create(OpCodes.Ret));
            }
            target.Body.MaxStackSize=Math.Max(target.Body.MaxStackSize,1);
        }
        After("NGame2.NUI.NWindow.LoginBackground","Init","CleanLoginBackground");
        After("NGame2.NUI.NWindow.LoginBackground","SetPatchVersion","HidePatchVersion");
        After("NGame2.NUI.NWindow.LoginAccountInfo","Refresh_Noraml","CleanLoginInfo");
        After("NGame2.NUI.NWindow.GameOptionWindow","SetupEtc","CleanSettings");
        After("NGame2.NUI.NWindow.LoginTouchWait","Init","ContinueAccountSwitch");
        After("NGame2.NAccount.AccountManager","Logout","ClearSession",false);
        After("NGame2.NLogin.LoginData","set_LastRefreshToken","SyncRefreshToken",false);
        // Stop queued lobby traffic as soon as the server confirms logout, before
        // the state coroutine advances. Otherwise it can use the revoked session.
        var logoutCallback=module.GetTypes().Where(t=>t.FullName.StartsWith("NGame2.NLogin.NState.StateBase_Logout/"))
            .SelectMany(t=>t.Methods).Single(m=>m.Parameters.Count==1 && m.Parameters[0].ParameterType.FullName=="NShared.Logout.Response");
        if(!logoutCallback.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkAccounts" && m.Name=="FinishLogout")) {
            var processor=logoutCallback.Body.GetILProcessor();var first=logoutCallback.Body.Instructions[0];
            processor.InsertBefore(first,Instruction.Create(OpCodes.Ldarg_1));
            processor.InsertBefore(first,Instruction.Create(OpCodes.Call,Bridge("FinishLogout")));
        }
        var settings=module.GetType("NGame2.NUI.NWindow.GameOptionWindow");
        // Remove the provider lookup coroutine; linking is no longer available.
        var setup=settings.Methods.Single(m=>m.Name=="SetupEtc");
        foreach(var call in setup.Body.Instructions.Where(i=>i.Operand is MethodReference m && m.Name=="OpenLinkedButton").ToArray()) {
            call.Previous.OpCode=OpCodes.Nop;call.Previous.Operand=null;
            call.OpCode=OpCodes.Nop;call.Operand=null;
            call.Next.OpCode=OpCodes.Nop;call.Next.Operand=null;
            call.Next.Next.OpCode=OpCodes.Pop;call.Next.Next.Operand=null;
        }
        foreach(var method in settings.Methods.Where(m=>m.Name is "ConnectToSocialId" or "OnClickCustomerCenter" or "OnClickPrivacyPolicy" or "OnClickTermsOfService")) {
            method.Body=new MethodBody(method);method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }
        var select=module.GetType("NGame2.NUI.NWindow.LoginSelect");
        var open=select.Methods.Single(m=>m.Name=="Open" && m.Parameters.Count==4);
        open.Body=new MethodBody(open);var il=open.Body.GetILProcessor();
        il.Append(Instruction.Create(OpCodes.Ldarg_0));
        foreach(var parameter in open.Parameters)il.Append(Instruction.Create(OpCodes.Ldarg,parameter));
        il.Append(Instruction.Create(OpCodes.Call,Bridge("Open")));il.Append(Instruction.Create(OpCodes.Ret));
        foreach(var method in select.Methods.Where(m=>m.Name.StartsWith("OnClick") && m.Name!="OnClickServerSelect" || m.Name=="LoginByAccountId")) {
            method.Body=new MethodBody(method);method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }
        var manager=module.GetType("NGame2.NAccount.MasangLoginManager");
        foreach(var name in new[]{"GuestLogin","SteamLogin","GoogleLogin","AppleLogin"}) {
            var method=manager.Methods.Single(m=>m.Name==name);method.Body=new MethodBody(method);
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Call,Bridge(name=="GuestLogin"?"Complete":"Reject")));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }
        var awake=manager.Methods.Single(m=>m.Name=="Awake");
        if(!awake.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkAccounts" && m.Name=="Initialize"))
            awake.Body.GetILProcessor().InsertBefore(awake.Body.Instructions[0],Instruction.Create(OpCodes.Call,Bridge("Initialize")));
        var guest=module.GetType("NGame2.NLogin.NState.StateBase_LoginMethod_Guest").Methods.Single(m=>m.Name=="NextState");
        foreach(var instruction in guest.Body.Instructions)
            if(instruction.OpCode==OpCodes.Newobj && instruction.Operand is MethodReference ctor && ctor.DeclaringType.Name=="StateBase_LoginGameServer" && instruction.Previous.OpCode==OpCodes.Ldc_I4_1)
                instruction.Previous.OpCode=OpCodes.Ldc_I4_0;
        RemoveLoginProviderReferences(module);
        var policy=Path.Combine(clientRoot,"King's Raid_Data","sprk-privacy.json");
        if(File.Exists(policy) && System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(policy))?["real_money_payments"]?.GetValue<bool>()==false)
            RemovePaymentReferences(module);
        var staged=dll+".accounts-staged";
        File.WriteAllBytes(Path.Combine(managed,"SprkAccounts.dll.staged"),bytes.ToArray());assembly.Write(staged);
        if(!stageOnly) {
            var backup=Path.Combine(Environment.CurrentDirectory,"target","account-login-backups",DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup);File.Copy(dll,Path.Combine(backup,"Assembly-CSharp.dll"));
            var helperPath=Path.Combine(managed,"SprkAccounts.dll");
            if(File.Exists(helperPath))File.Copy(helperPath,Path.Combine(backup,"SprkAccounts.dll"));
            assembly.Dispose();File.Move(Path.Combine(managed,"SprkAccounts.dll.staged"),helperPath,true);File.Move(staged,dll,true);
        }
        Console.WriteLine(stageOnly?"Staged username/password login form.":"Installed username/password login form. Restart the client to load it.");
    }
}
