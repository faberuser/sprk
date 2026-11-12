using Mono.Cecil;
using System.Security.Cryptography;
using System.Text.Json;

namespace DllPatcher;
partial class Program {
    static void VerifyLoginPresentation(ModuleDefinition game,ModuleDefinition helper) {
        var presentation=helper.GetType("SprkLoginPresentation") ?? throw new InvalidOperationException("Login presentation helper missing");
        foreach(var name in new[]{"SelectBackground","SelectMusic","FilterMusic","Configure","Setup","ApplyBackground","LoadMusic","NormalizeBackgroundId","NormalizeMusicId","RestoreLogoSubtitle","ApplyEraLogo","CoverScreen","CheckEdgePixels","AlignLogo","AlignMessage"})
            if(!presentation.Methods.Any(m=>m.Name==name))throw new InvalidOperationException("Missing login presentation method: "+name);
        foreach(var pair in new[]{("CleanLoginBackground","Configure"),("CleanSettings","Setup")})
            if(helper.GetType("SprkAccounts").Methods.Single(m=>m.Name==pair.Item1).Body.Instructions.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkLoginPresentation" && m.Name==pair.Item2)!=1)
                throw new InvalidOperationException("Missing or duplicated login presentation integration: "+pair.Item2);
        var music=game.GetType("NGameAsset.BgmManager2").Methods.Single(m=>m.Name=="PlayFromResources");
        if(music.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkLoginPresentation" && m.Name=="FilterMusic")!=1)
            throw new InvalidOperationException("Native login BGM filter missing or duplicated");
        VerifyArenaResultStack(music);
        var phase=game.GetType("NGameAsset.NEffect.ParticleInstance").Methods.Single(m=>m.Name=="PlayPhase");
        if(phase.Body.Instructions[1].Operand is not FieldReference field || field.Name!="SequenceQueue")throw new InvalidOperationException("Recovered particle phase guard missing");
        if(phase.Body.Instructions[2].OpCode.Code!=Mono.Cecil.Cil.Code.Brtrue || phase.Body.Instructions[3].OpCode.Code!=Mono.Cecil.Cil.Code.Ret || !ReferenceEquals(phase.Body.Instructions[2].Operand,phase.Body.Instructions[4]))throw new InvalidOperationException("Invalid particle phase guard branch");
        foreach(var pair in new[]{("SelectBackground","Sprk.Login.Background"),("SelectMusic","Sprk.Login.Music")}) {
            var method=presentation.Methods.Single(m=>m.Name==pair.Item1);
            if(!method.Body.Instructions.Any(i=>Equals(i.Operand,pair.Item2)) ||
               !method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkTestPrefs" && m.Name=="Save"))
                throw new InvalidOperationException("Missing independent persisted login choice: "+pair.Item1);
        }
        var menu=helper.GetType("SprkLoginPresentationMenu");
        if(menu.Methods.Single(m=>m.Name=="OpenPicker").Body.Instructions.Any(i=>Equals(i.Operand,"Current Background")))throw new InvalidOperationException("Duplicate Current Background choice remains");
        if(!presentation.Methods.Single(m=>m.Name=="get_SelectedBackground").Body.Instructions.Any(i=>Equals(i.Operand,"x-the-final-azeta-and-valance")))throw new InvalidOperationException("Azeta and Valance default background is missing");
        if(!presentation.Methods.Single(m=>m.Name=="get_SelectedMusic").Body.Instructions.Any(i=>Equals(i.Operand,"e47c58d47a86adfa0c40")))throw new InvalidOperationException("Promise default login music is missing");
        if(menu.Methods.Single(m=>m.Name=="OpenPicker").Body.Instructions.Any(i=>Equals(i.Operand,"Default Login Music")))throw new InvalidOperationException("Removed Default Login Music choice remains");
        foreach(var caption in new[]{"Button_LoginBackground","Button_LoginMusic"})
            if(!menu.Methods.Single(m=>m.Name=="Setup").Body.Instructions.Any(i=>Equals(i.Operand,caption)))throw new InvalidOperationException("Missing Others control: "+caption);
        var closeToTitle=helper.GetType("SprkAccounts").Methods.SingleOrDefault(m=>m.Name=="CloseToTitle") ?? throw new InvalidOperationException("Account form close button missing");
        if(!closeToTitle.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name==".ctor" && m.DeclaringType.Name=="StateBase_WaitTouch"))throw new InvalidOperationException("Account form close does not return to TAP TO PLAY");
        if(!helper.GetType("SprkTapToPlay").Methods.Any(m=>m.Name=="TryGetLineWorldY"))throw new InvalidOperationException("TAP TO PLAY line is not shared with the status message");
        if(helper.GetType("SprkRecoveredLoginScene")==null)throw new InvalidOperationException("Recovered scene loader missing");
        Console.WriteLine("Verified independent login background/music preferences, Others controls, wide-screen cover, account-form close and native BGM integration.");
    }
    static void VerifyLoginPresentationAssets(string root) {
        VerifyNoCenterLoginLogos(root);
        string directory=Path.Combine(root,"King's Raid_Data","Documents","Patch","StandaloneWindows","LoginPresentation");
        string prefix=Path.GetFullPath(directory)+Path.DirectorySeparatorChar;
        using var integrity=JsonDocument.Parse(File.ReadAllText(Path.Combine(directory,"integrity.json")));
        foreach(var entry in integrity.RootElement.EnumerateObject()) {
            string path=Path.GetFullPath(Path.Combine(directory,entry.Name));
            if(!path.StartsWith(prefix,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Login asset escapes managed directory");
            if(new FileInfo(path).Length!=entry.Value.GetProperty("size").GetInt64())throw new InvalidOperationException("Login asset size mismatch: "+entry.Name);
            using var stream=File.OpenRead(path);
            if(!Convert.ToHexString(SHA256.HashData(stream)).Equals(entry.Value.GetProperty("sha256").GetString(),StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Login asset hash mismatch: "+entry.Name);
        }
        using var catalog=JsonDocument.Parse(File.ReadAllText(Path.Combine(directory,"catalog.json")));
        foreach(var key in new[]{"backgrounds","music"}) {
            var ids=new HashSet<string>();
            foreach(var item in catalog.RootElement.GetProperty(key).EnumerateArray()) {
                if(!ids.Add(item.GetProperty("id").GetString()!))throw new InvalidOperationException("Duplicate login catalog id");
                string relative=item.GetProperty(key=="backgrounds"?"scene":"file").GetString()!;
                if(!integrity.RootElement.TryGetProperty(relative,out _))throw new InvalidOperationException("Unverified login catalog asset: "+relative);
                if(key=="backgrounds" && item.TryGetProperty("logo",out var logo) && !integrity.RootElement.TryGetProperty(logo.GetString()!,out _))throw new InvalidOperationException("Unverified login logo: "+logo.GetString());
            }
            if(ids.Count==0)throw new InvalidOperationException("Empty login catalog: "+key);
        }
        Console.WriteLine("Verified login presentation catalog and all managed asset sizes/SHA-256.");
    }
}
