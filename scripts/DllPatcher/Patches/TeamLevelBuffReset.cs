using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

// TeamLevelBuffManager is a global singleton. Init runs on every first lobby,
// including after Switch Account, and re-adds the current account's buffs keyed
// by OpenBuffLevel without removing old entries. The previous account's higher
// buff levels therefore stayed in the Total Applied Buff popup and stat lists.
// Clear the caches first, as PetManager.RefreshAllPetBuff does (ClearCachedDic).
partial class Program
{
    static readonly string[] TeamLevelBuffCaches =
        { "_cachedTeamLevelBuffStatDataDic", "_cachedTeamLevelBuffContentsStatDataDic", "_cachedTeamLevelBonusDataDic" };

    static MethodDefinition TeamLevelBuffInit(ModuleDefinition module) => module
        .GetType("NGame2.TeamLevelBuffManager").Methods.Single(m => m.Name == "Init" && !m.HasParameters);

    static bool ClearsTeamLevelBuffCache(Instruction load) =>
        load.OpCode == OpCodes.Ldfld && load.Next?.OpCode == OpCodes.Callvirt
        && load.Next.Operand is MethodReference { Name: "Clear" } clear && clear.DeclaringType.FullName == ((FieldReference)load.Operand).FieldType.FullName;

    static void PatchTeamLevelBuffReset(ModuleDefinition module)
    {
        var init = TeamLevelBuffInit(module);
        var code = init.Body.Instructions;
        if (code.Take(3 * TeamLevelBuffCaches.Length).Count(i => i.Operand is FieldReference f && TeamLevelBuffCaches.Contains(f.Name) && ClearsTeamLevelBuffCache(i)) == TeamLevelBuffCaches.Length)
            return;
        var il = init.Body.GetILProcessor();
        var first = code[0];
        foreach (var name in TeamLevelBuffCaches)
        {
            var field = init.DeclaringType.Fields.Single(f => f.Name == name);
            var dictionary = (GenericInstanceType)field.FieldType;
            var clear = new MethodReference("Clear", module.TypeSystem.Void, dictionary) { HasThis = true };
            il.InsertBefore(first, il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first, il.Create(OpCodes.Ldfld, field));
            il.InsertBefore(first, il.Create(OpCodes.Callvirt, module.ImportReference(clear)));
        }
        // Prepended code shifts later short branches; widen them.
        foreach (var i in code)
            if (i.OpCode.OperandType == OperandType.ShortInlineBrTarget)
                i.OpCode = i.OpCode.Code switch
                {
                    Code.Br_S => OpCodes.Br, Code.Brtrue_S => OpCodes.Brtrue, Code.Brfalse_S => OpCodes.Brfalse,
                    Code.Beq_S => OpCodes.Beq, Code.Bne_Un_S => OpCodes.Bne_Un, Code.Blt_S => OpCodes.Blt, Code.Bge_S => OpCodes.Bge,
                    Code.Bgt_S => OpCodes.Bgt, Code.Ble_S => OpCodes.Ble, Code.Leave_S => OpCodes.Leave,
                    _ => throw new InvalidOperationException("Unexpected short branch in TeamLevelBuffManager.Init: " + i.OpCode)
                };
        VerifyTeamLevelBuffReset(module);
    }

    static void VerifyTeamLevelBuffReset(ModuleDefinition module)
    {
        var code = TeamLevelBuffInit(module).Body.Instructions;
        for (var i = 0; i < TeamLevelBuffCaches.Length; i++)
        {
            var load = code[i * 3 + 1];
            if (code[i * 3].OpCode != OpCodes.Ldarg_0 || load.Operand is not FieldReference { } f || f.Name != TeamLevelBuffCaches[i] || !ClearsTeamLevelBuffCache(load))
                throw new InvalidOperationException("TeamLevelBuffManager.Init must clear " + TeamLevelBuffCaches[i] + " before rebuilding.");
        }
        // The cached buffs are rebuilt from the current account afterwards.
        if (!code.Skip(9).Any(i => i.Operand is MethodReference { Name: "AddOrUpdateTeamLevelBuff" })
            || !code.Skip(9).Any(i => i.Operand is MethodReference { Name: "AddOrUpdateTeamLevelBonusBuff" }))
            throw new InvalidOperationException("TeamLevelBuffManager.Init no longer rebuilds team level buffs.");
        if (code.Any(i => i.OpCode.OperandType == OperandType.ShortInlineBrTarget))
            throw new InvalidOperationException("TeamLevelBuffManager.Init kept a short branch.");
        // Each prepended triple (ldarg.0; ldfld; callvirt Clear) is stack-neutral
        // and checked above. VerifyArenaResultStack does not model this method's
        // native try/finally enumerator, so it is not used here.
    }

    static void InstallTeamLevelBuffReset(string clientRoot, bool verifyOnly)
    {
        var dll = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed", "Assembly-CSharp.dll"));
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(dll)!);
        using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver }))
        {
            if (verifyOnly)
            {
                VerifyTeamLevelBuffReset(assembly.MainModule);
                resolver.Dispose();
                Console.WriteLine("Verified team level buffs are rebuilt from the current account on every lobby entry.");
                return;
            }
            if (UsesAnyClient(clientRoot)) throw new InvalidOperationException("Close game clients before installing the team level buff reset.");
            PatchTeamLevelBuffReset(assembly.MainModule);
            var count = TeamLevelBuffInit(assembly.MainModule).Body.Instructions.Count;
            PatchTeamLevelBuffReset(assembly.MainModule);
            if (count != TeamLevelBuffInit(assembly.MainModule).Body.Instructions.Count)
                throw new InvalidOperationException("Team level buff reset patch is not idempotent.");
            var backup = Path.Combine("target", "team-level-buff-reset-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup);
            File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
            assembly.Write(dll + ".team-level-buff-staged");
        }
        resolver.Dispose();
        File.Move(dll + ".team-level-buff-staged", dll, true);
        InstallTeamLevelBuffReset(clientRoot, true);
    }

    static bool UsesAnyClient(string clientRoot) => System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, clientRoot));
}
