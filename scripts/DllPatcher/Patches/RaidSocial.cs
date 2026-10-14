using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static void PatchRaidSocial(ModuleDefinition module)
    {
        // RaidInviteComponent cached IsOnline at Open, while presence handlers
        // update the shared CachedUserInfo. Read that object on every refresh.
        var component = module.GetType("NGame2.NUI.NComponent2.RaidInviteComponent");
        var update = component.Methods.Single(m => m.Name == "UpdateButtonState");
        var info = component.Methods.Single(m => m.Name == "get_UserInfo");
        var online = module.GetType("NShared.CachedUserInfo").Methods.Single(m => m.Name == "get_IsOnline");
        foreach (var read in update.Body.Instructions.Where(i => i.OpCode == OpCodes.Ldfld &&
            i.Operand is FieldReference f && f.Name == "_isOnline").ToArray()) {
            read.OpCode = OpCodes.Call;
            read.Operand = info;
            update.Body.GetILProcessor().InsertAfter(read, Instruction.Create(OpCodes.Callvirt, online));
        }
        if (update.Body.Instructions[1].Operand is not MethodReference guard || guard.Name != "get_UserInfo") {
            var il = update.Body.GetILProcessor();
            var first = update.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, info));
            il.InsertBefore(first, Instruction.Create(OpCodes.Brtrue, first));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ret));
        }
        VerifyRaidSocial(module);
    }

    static void VerifyRaidSocial(ModuleDefinition module)
    {
        var method = module.GetType("NGame2.NUI.NComponent2.RaidInviteComponent").Methods.Single(m => m.Name == "UpdateButtonState");
        if (method.Body.Instructions.Any(i => i.OpCode == OpCodes.Ldfld && i.Operand is FieldReference f && f.Name == "_isOnline") ||
            !method.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "get_UserInfo") ||
            !method.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "get_IsOnline"))
            throw new InvalidOperationException("Raid invite list does not read live presence.");
    }

    static void VerifyRaidSocialFile(string clientRoot)
    {
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(clientRoot, "King's Raid_Data", "Managed", "Assembly-CSharp.dll"));
        VerifyRaidSocial(assembly.MainModule);
        Console.WriteLine("Verified raid invite list reads live presence.");
    }

    static void InstallRaidSocial(string clientRoot, bool stageOnly)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var staged = dll + ".raid-social-staged";
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver })) {
            PatchRaidSocial(assembly.MainModule);
            assembly.Write(staged);
        }
        using (var verify = AssemblyDefinition.ReadAssembly(staged)) VerifyRaidSocial(verify.MainModule);
        if (!stageOnly) {
            var backup = Path.Combine("target", "raid-social-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup);
            File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
            File.Move(staged, dll, true);
        }
        Console.WriteLine(stageOnly ? "Staged and verified raid presence fix." : "Installed and verified raid presence fix. Restart the client.");
    }
}
