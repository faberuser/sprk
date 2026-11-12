using Mono.Cecil;
namespace DllPatcher;
partial class Program {
    static void VerifyAccountSession(string clientRoot) {
        var managed=Path.Combine(clientRoot,"King's Raid_Data","Managed");
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkAccounts.dll"));
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"));
        var bridge=helper.MainModule.GetType("SprkAccounts");
        bool Calls(MethodDefinition method,string type,string name)=>method.HasBody && method.Body.Instructions
            .Any(i=>i.Operand is MethodReference call && call.DeclaringType.Name==type && call.Name==name);
        void Require(bool condition,string message) {if(!condition)throw new InvalidOperationException(message);}
        var clear=bridge.Methods.Single(m=>m.Name=="ClearSession");
        Require(!Calls(clear,"SprkAccountStore","Remove"),"Session cleanup must preserve saved accounts.");
        Require(Calls(clear,"SprkAccounts","SyncRefreshToken") && Calls(clear,"SprkAccounts","ReleaseAccount"),"Cleanup must save renewed tokens and release the account lease.");
        var forget=bridge.Methods.Single(m=>m.Name=="ForgetSession");
        Require(Calls(forget,"SprkAccountStore","Remove") && Calls(forget,"SprkAccounts","ClearSession"),"Explicit logout must forget the account and clear runtime state.");
        Require(Calls(bridge.Methods.Single(m=>m.Name=="FinishLogout"),"SprkAccounts","ForgetSession"),"Confirmed logout must forget the account.");
        Require(helper.MainModule.GetTypes().Where(t=>t.Name.Contains("DeleteCurrentAccount")).SelectMany(t=>t.Methods)
            .Any(m=>Calls(m,"SprkAccounts","ForgetSession")),"Account deletion must forget the account.");
        Require(Calls(game.MainModule.GetType("NGame2.NAccount.AccountManager").Methods.Single(m=>m.Name=="Logout"),"SprkAccounts","ClearSession"),"Native cleanup hook missing.");
        var reloads=game.MainModule.GetTypes().Where(t=>t.FullName.Contains("LobbyManagement")).SelectMany(t=>t.Methods)
            .Where(m=>Calls(m,"SystemOptionData","set_Spec") && Calls(m,"AccountManager","Logout")).ToArray();
        Require(reloads.Length>0,"Expected graphics reload cleanup path missing.");
        Console.WriteLine("Verified installed account session cleanup, token sync, lease release, explicit logout, account deletion and graphics reload path.");
    }
}
