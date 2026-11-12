using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static IEnumerable<MethodDefinition> CompletionBannerMethods(ModuleDefinition module)
    {
        MethodDefinition Find(string type, string name, params string[] parameters) => module.GetType(type)
            .Methods.Single(m => m.Name == name && m.ReturnType.FullName == "System.Void" &&
                m.Parameters.Select(p => p.ParameterType.FullName).SequenceEqual(parameters));
        yield return Find("NGame2.NUI.NManager.AchievementClearedPopupQueue", "ReserveAchievementClearedPopUp",
            "NShared.AchievementData", "System.Boolean");
        yield return Find("NGame2.NUI.NWindow.AchievementClearedPopup", "Open", "NShared.AchievementData",
            "NGame2.NUI.NWindow.AchievementClearedPopup/ITextProvider", "NGame2.NUI.NWindow.AchievementClearedPopup/Listener");
        yield return Find("NGame2.NUI.NWindow.ConfirmNotiPopup", "Open",
            "System.String", "System.String", "System.String", "System.Int32");
        const string queue = "NGame2.NUI.NManager.NLobby.LobbyPopupQueue";
        // Disable only notification reservations. Keep the outer methods that
        // consume the pending-notification lists and process content unlocks.
        yield return Find(queue, "ReservedMainQuestClearPopup", "System.Int32");
        yield return Find(queue, "ReserveSubQuestClearPopup", "NShared.SubQuestInfo");
        yield return Find(queue, "ReserveSubQuestOpenPopup", "NShared.SubQuestData");
    }

    static void PatchCompletionBanners(ModuleDefinition module)
    {
        // Legacy achievements and newer main/submissions use different banners.
        // Prevent both rendering and reservation so notifications cannot hold up
        // the lobby queue before the awakening cinematic.
        foreach (var method in CompletionBannerMethods(module)) {
            method.Body = new MethodBody(method);
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }
        VerifyCompletionBanners(module);
    }

    static void VerifyCompletionBanners(ModuleDefinition module)
    {
        foreach (var method in CompletionBannerMethods(module)) {
            var body = method.Body;
            if (body.Instructions.Count != 1 || body.Instructions[0].OpCode != OpCodes.Ret ||
                body.Variables.Count != 0 || body.ExceptionHandlers.Count != 0)
                throw new InvalidOperationException("Completion banner must return without creating or queuing a popup: " + method.FullName);
        }
    }

    static void InstallCompletionBanners(string clientRoot, bool stageOnly)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var staged = dll + ".completion-banners-staged";
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver })) {
            PatchCompletionBanners(assembly.MainModule);
            assembly.Write(staged);
        }
        using (var verified = AssemblyDefinition.ReadAssembly(staged)) VerifyCompletionBanners(verified.MainModule);
        if (!stageOnly) {
            var backup = Path.Combine("target", "completion-banners-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup);
            File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
            File.Move(staged, dll, true);
        }
        Console.WriteLine(stageOnly ? "Staged and verified disabled completion banners." :
            "Installed and verified disabled completion banners. Restart the client.");
    }
}
