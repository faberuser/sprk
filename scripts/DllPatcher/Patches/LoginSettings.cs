using Mono.Cecil;
using Mono.Cecil.Cil;
namespace DllPatcher;
partial class Program {
    static void VerifyLoginSettings(ModuleDefinition game,ModuleDefinition helper) {
        var accounts=helper.GetType("SprkAccounts");
        var clean=accounts.Methods.Single(m=>m.Name=="CleanSettings");
        if(clean.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.Name=="ArrangeAccountButtons")!=1)throw new InvalidOperationException("Missing Others account button layout");
        var layout=accounts.Methods.Single(m=>m.Name=="ArrangeAccountButtons");
        foreach(var name in new[]{"Button_ChangeLanguage","Button_ChangePassword","Button_DeleteAccount","Button_SwitchAccount","Common_Button7_Cancel"})
            if(!layout.Body.Instructions.Any(i=>Equals(i.Operand,name)))throw new InvalidOperationException("Missing Others control/artwork: "+name);
        foreach(var pair in new[]{("CleanLoginInfo","Configure"),("CleanSettings","CleanSettings")})
            if(!accounts.Methods.Single(m=>m.Name==pair.Item1).Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkLoginSettings" && m.Name==pair.Item2))throw new InvalidOperationException("Missing login settings integration: "+pair.Item1);
        var bridge=helper.GetType("SprkLoginSettings");
        var configure=bridge.Methods.Single(m=>m.Name=="Configure");
        foreach(var name in new[]{"PatchValidationRoot","VoiceButtonRoot","LanguageButtonRoot"})
            if(!configure.Body.Instructions.Any(i=>i.Operand is FieldReference f && f.Name==name))throw new InvalidOperationException("Missing login button cleanup: "+name);
        if(!configure.Body.Instructions.Any(i=>Equals(i.Operand,"Settings")))throw new InvalidOperationException("Missing Settings label");
        if(!bridge.Methods.Single(m=>m.Name=="OpenSettings").Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="Open" && m.DeclaringType.Name=="GameOptionWindow"))throw new InvalidOperationException("Login must open the native settings panel");
        if(!bridge.Methods.Single(m=>m.Name=="OpenSettings").Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="SetEnableLoadFromPatch"))throw new InvalidOperationException("Login settings must load installed full localization.");
        var atlasLoader=bridge.Methods.Single(m=>m.Name=="TryLoginAtlas");
        foreach(var name in new[]{"GameOption","PayShop"})
            if(!atlasLoader.Body.Instructions.Any(i=>Equals(i.Operand,name)))throw new InvalidOperationException("Missing login settings atlas: "+name);
        if(!bridge.Methods.Single(m=>m.Name=="LoadLoginAtlas").Body.Instructions.Any(i=>Equals(i.Operand,"Unlit/Transparent Colored")))throw new InvalidOperationException("Login atlases must use an available NGUI shader.");
        foreach(var pair in new[]{("NGame2.NUI.NWindow.GameOptionWindow","Open","PrepareSettings"),("NGame2.NUI.NAtlasAssignComponent.AtlasAssignerBase","GetAtlas","TryLoginAtlas")}) {
            var method=game.GetType(pair.Item1).Methods.Single(m=>m.Name==pair.Item2 && (pair.Item2!="Open" || m.Parameters.Count==3));
            if(method.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkLoginSettings" && m.Name==pair.Item3)!=1)throw new InvalidOperationException("Missing or duplicated settings integration: "+pair.Item3);
            VerifyArenaResultStack(method);
        }
        foreach(var name in new[]{"SetTextureQuality","SetPlayBGM"}) {
            var method=game.GetType("NGame2.NUI.NComponent2.GameOptionBaseComponent").Methods.Single(m=>m.Name==name);
            if(method.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkLoginSettings" && m.Name==name)!=1)throw new InvalidOperationException("Missing or duplicated login callback: "+name);
            VerifyArenaResultStack(method);
        }
        Console.WriteLine("Verified login Settings button, removed patch/voice buttons and native panel integration.");
    }
    static void VerifyLoginSettingsFile(string root,bool staged=false) {
        var managed=Path.Combine(root,"King's Raid_Data","Managed");
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"+(staged?".accounts-staged":"")));
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkAccounts.dll"+(staged?".staged":"")));
        VerifyLoginSettings(game.MainModule,helper.MainModule);
    }
}
