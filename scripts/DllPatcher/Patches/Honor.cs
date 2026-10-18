using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static MethodDefinition HonorWeeklyRewardMethod(ModuleDefinition module) => module
        .GetType("NGame2.NUI.NComponent2.MatchBanPickTierRewardComponent")
        .Methods.Single(m => m.Name == "SetWeeklyBoxReward");

    static void PatchHonor(ModuleDefinition module)
    {
        var method = HonorWeeklyRewardMethod(module);
        var lookup = method.Body.Instructions.Single(i => i.Operand is MethodReference m &&
            m.DeclaringType.FullName.Contains("NShared.RewardData>") && m.Name == "GetData");
        if (lookup.Next.OpCode != OpCodes.Dup)
        {
            var root = method.Body.Instructions.Select(i => i.Operand).OfType<FieldReference>()
                .First(f => f.Name == "WeeklyRewardRoot");
            var hide = method.Body.Instructions.Select(i => i.Operand).OfType<MethodReference>()
                .First(m => m.Name == "EnableGameObject");
            var next = lookup.Next;
            var il = method.Body.GetILProcessor();
            // Archived Honor weekly boxes can reference absent reward rows.
            // Keep the tier/rank panel usable and hide the unavailable box.
            foreach (var instruction in new[] {
                Instruction.Create(OpCodes.Dup), Instruction.Create(OpCodes.Brtrue, next),
                Instruction.Create(OpCodes.Pop), Instruction.Create(OpCodes.Ldarg_2),
                Instruction.Create(OpCodes.Ldfld, root), Instruction.Create(OpCodes.Ldc_I4_0), Instruction.Create(OpCodes.Ldc_I4_0),
                Instruction.Create(OpCodes.Call, hide), Instruction.Create(OpCodes.Ret)
            }) il.InsertBefore(next, instruction);
        }
        var result = module.GetType("NGame2.NUI.NWindow.NBattle.PvpResult");
        var init = result.Methods.Single(m => m.Name == "Init_BanPickOnly");
        if (!init.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "OpenRewardPopup"))
        {
            var normal = result.Methods.Single(m => m.Name == "Init_Pvp");
            var popup = normal.Body.Instructions.Single(i => i.Operand is MethodReference m && m.Name == "OpenRewardPopup");
            var field = (FieldReference)popup.Next.Operand;
            var hide = normal.Body.Instructions.Last(i => i.Operand is MethodReference m && m.Name == "EnableGameObject");
            var il = init.Body.GetILProcessor();
            var end = init.Body.Instructions.Last();
            // Match the normal result screen: create the receipt hidden, then
            // let UpdateEffectEnd reveal it after the victory/defeat animation.
            foreach (var instruction in new[] {
                Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldarg_1),
                Instruction.Create(OpCodes.Call, (MethodReference)popup.Operand), Instruction.Create(OpCodes.Stfld, field),
                Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldfld, field),
                Instruction.Create(OpCodes.Ldc_I4_0), Instruction.Create(OpCodes.Ldc_I4_0),
                Instruction.Create(OpCodes.Call, (MethodReference)hide.Operand)
            }) il.InsertBefore(end, instruction);
        }
        VerifyHonor(module);
    }

    static void VerifyHonor(ModuleDefinition module)
    {
        var init = module.GetType("NGame2.NUI.NWindow.NBattle.PvpResult").Methods.Single(m => m.Name == "Init_BanPickOnly");
        VerifyArenaResultStack(init);
        if (init.Body.Instructions.Count(i => i.Operand is MethodReference m && m.Name == "OpenRewardPopup") != 1 ||
            !init.Body.Instructions.Any(i => i.OpCode == OpCodes.Stfld && i.Operand is FieldReference f && f.Name == "_rewardPopup"))
            throw new InvalidOperationException("Honor results must open the currency and item reward receipt exactly once.");
        var method = HonorWeeklyRewardMethod(module);
        VerifyArenaResultStack(method);
        var lookup = method.Body.Instructions.Single(i => i.Operand is MethodReference m &&
            m.DeclaringType.FullName.Contains("NShared.RewardData>") && m.Name == "GetData");
        var guard = method.Body.Instructions.Skip(method.Body.Instructions.IndexOf(lookup) + 1).Take(9).ToArray();
        if (guard[0].OpCode != OpCodes.Dup || guard[1].OpCode != OpCodes.Brtrue || guard[1].Operand != guard[8].Next ||
            guard[2].OpCode != OpCodes.Pop || guard[3].OpCode != OpCodes.Ldarg_2 ||
            guard[4].Operand is not FieldReference field || field.Name != "WeeklyRewardRoot" ||
            guard[5].OpCode != OpCodes.Ldc_I4_0 || guard[6].OpCode != OpCodes.Ldc_I4_0 || guard[7].Operand is not MethodReference hide || hide.Name != "EnableGameObject" ||
            guard[8].OpCode != OpCodes.Ret || !method.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "get_Item1CountMin"))
            throw new InvalidOperationException("Honor reward lookup must hide missing boxes and retain valid reward rendering.");
    }

    static void VerifyHonorFile(string clientRoot, bool staged)
    {
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(clientRoot,"King's Raid_Data","Managed","Assembly-CSharp.dll" + (staged ? ".honor-staged" : "")));
        VerifyHonor(assembly.MainModule);
        Console.WriteLine("Verified Honor duel payout display, weekly reward fallback and IL control flow.");
    }

    static void InstallHonor(string clientRoot, bool stageOnly)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot,"King's Raid_Data","Managed"));
        var dll = Path.Combine(managed,"Assembly-CSharp.dll");
        var staged = dll + ".honor-staged";
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver }))
        {
            PatchHonor(assembly.MainModule);
            assembly.Write(staged);
        }
        using (var verified = AssemblyDefinition.ReadAssembly(staged, new ReaderParameters { AssemblyResolver = resolver }))
        {
            VerifyHonor(verified.MainModule);
            PatchHonor(verified.MainModule);
            using var repeated = new MemoryStream();
            verified.Write(repeated);
            if (!repeated.ToArray().SequenceEqual(File.ReadAllBytes(staged)))
                throw new InvalidOperationException("Honor patch must be idempotent.");
        }
        if (!stageOnly)
        {
            var backup = Path.Combine("target","honor-backups",DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup);
            File.Copy(dll,Path.Combine(backup,"Assembly-CSharp.dll"));
            File.Move(staged,dll,true);
        }
        Console.WriteLine(stageOnly ? "Staged and verified Honor reward fix." : "Installed Honor reward fix. Restart the client.");
    }
}
