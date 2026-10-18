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
        VerifyHonor(module);
    }

    static void VerifyHonor(ModuleDefinition module)
    {
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
        Console.WriteLine("Verified Honor weekly reward fallback and IL control flow.");
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
