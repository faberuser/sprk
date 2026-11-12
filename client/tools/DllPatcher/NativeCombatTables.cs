using System.Security.Cryptography;
using System.Text.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

// A baked client keeps the original restoration algorithms, while its native
// tables supply the data. Repatching must not reintroduce the old data payload.
public static class NativeCombatTables
{
    public const string ManifestName = "SPRK-NativeCombat.json";
    public const string EmptyPayload = "{\"Tables\":{\"Creature\":{},\"State\":{},\"SkillProjectile\":{},\"Constant\":{}},\"Skills\":[],\"Operations\":[],\"MissingOperations\":[],\"MissingStates\":[]}";

    public static bool ValidateInstalled(string managedDirectory)
    {
        var patchRoot = Path.GetFullPath(Path.Combine(managedDirectory, "../Documents/Patch/StandaloneWindows"));
        return ValidatePatchRoot(patchRoot);
    }

    public static bool ValidatePatchRoot(string patchRoot)
    {
        patchRoot = Path.GetFullPath(patchRoot);
        var manifestPath = Path.Combine(patchRoot, ManifestName);
        if (!File.Exists(manifestPath)) return false;
        using var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));
        if (manifest.RootElement.GetProperty("version").GetInt32() != 1)
            throw new InvalidOperationException("Unsupported native combat manifest version.");
        var files = manifest.RootElement.GetProperty("files");
        foreach (var name in new[] { "Skill", "State", "Creature", "SkillLevelFactor", "CreatureStarGradeStat", "SoulWeaponOption", "MonsterTier", "BattleDefine", "Constant", "SkillProjectile" })
            if (!files.TryGetProperty($"TableJit/{name}Table.jit", out _))
                throw new InvalidOperationException($"Native combat manifest is missing {name}Table.jit.");
        if (!files.TryGetProperty("LocalizationJit/LocalizationAll_English.jit", out _))
            throw new InvalidOperationException("Native combat manifest is missing English localization.");
        foreach (var file in files.EnumerateObject())
        {
            var path = Path.GetFullPath(Path.Combine(patchRoot, file.Name));
            if (!path.StartsWith(patchRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Invalid native combat manifest path.");
            var expected = file.Value.GetProperty("sha256").GetString();
            if (!File.Exists(path) || !Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).Equals(expected, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Baked native table changed: {file.Name}. Rebuild the baked data before patching.");
        }
        return true;
    }

    public static void Apply(ModuleDefinition module)
    {
        var portal = module.Types.Single(t => t.Name == "RestoredPortal");
        var returns = new Dictionary<string, int> {
            ["PunishmentSkill"]=-2, ["CombatCreature"]=-2, ["CombatState"]=-2, ["CombatProjectile"]=-2,
            ["RestoreMissingCombatOperation"]=0, ["CombatRowNumber"]=0, ["CombatGradeStat"]=0, ["CombatConstant"]=0,
            ["FinalizeCombatContainer"]=-1, ["RestorePunishmentOperation"]=-1, ["InitializeCombatConstants"]=-1,
            ["IsRemovedPunishmentOperation"]=-3
        };
        foreach (var entry in returns)
        {
            var method = portal.Methods.Single(m => m.Name == entry.Key);
            var old = method.Body;
            method.Body = new MethodBody(method);
            var il = method.Body.GetILProcessor();
            if (entry.Value == -2)
            {
                var lookup = (MethodReference)old.Instructions.First(i => i.Operand is MethodReference m && m.Name == "GetData").Operand;
                il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldarg_1); il.Emit(OpCodes.Callvirt, lookup);
            }
            else if (entry.Value == 0) il.Emit(OpCodes.Ldarg_0);
            else if (entry.Value == -3) il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ret);
        }
        var replaced = 0;
        foreach (var instruction in portal.Methods.Single(m => m.IsConstructor && m.IsStatic).Body.Instructions)
            if (instruction.OpCode == OpCodes.Ldstr && instruction.Operand is string text
                && (text == EmptyPayload || text.Contains("\"MissingOperations\"") && text.Contains("\"SkillLevelFactor\"")))
            {
                instruction.Operand = EmptyPayload;
                replaced++;
            }
        if (replaced != 1) throw new InvalidOperationException("Expected exactly one combat restoration payload.");
        Console.WriteLine("Native combat tables are authoritative; disabled runtime data overrides and retained combat algorithms.");
    }
}
