using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;
using System.Runtime.Loader;

namespace DllPatcher;

partial class Program
{
    static bool PatchDamageReduction(ModuleDefinition module)
    {
        // Match the reducer by its two accumulator fields and GiveDamage callback,
        // rather than relying on compiler-generated display-class numbers.
        var stat = module.Types.Single(t => t.FullName == "NShared.StatController");
        var method = stat.NestedTypes
            .Where(t => t.Fields.Any(f => f.Name == "multedDamagePower") &&
                        t.Fields.Any(f => f.Name == "addedDamagePower"))
            .SelectMany(t => t.Methods)
            .Single(m => m.Name.StartsWith("<GiveDamage>") && m.HasBody &&
                m.Body.Instructions.Any(i => i.Operand is MethodReference r &&
                    r.Name == "AccumulateAmplifyDamage"));
        var code = method.Body.Instructions;
        var call = code.Single(i => i.Operand is MethodReference r && r.Name == "AccumulateAmplifyDamage");
        int index = code.IndexOf(call);
        if (method.Body.Variables.Count != 2 ||
            method.Body.Variables.Any(v => v.VariableType.FullName != "System.Int32") ||
            code[0].OpCode != OpCodes.Ldc_I4_0 || code[1].OpCode != OpCodes.Stloc_0 ||
            code[2].OpCode != OpCodes.Ldc_I4_0 || code[3].OpCode != OpCodes.Stloc_1 ||
            code[index + 2].OpCode != OpCodes.Ldloc_0 ||
            code[index + 4].OpCode != OpCodes.Bgt_S ||
            code[index + 5].OpCode != OpCodes.Ldloc_1 ||
            code[index + 7].OpCode != OpCodes.Bgt_S)
            throw new InvalidOperationException("Unrecognized damage reducer layout; refusing to patch.");

        string[] fields = { "multedDamagePower", "addedDamagePower" };
        bool original = true, patched = true;
        for (int n = 0; n < 2; n++)
        {
            var receiver = code[index - 4 + n * 2];
            var address = code[index - 3 + n * 2];
            original &= receiver.OpCode == OpCodes.Ldarg_0 && address.OpCode == OpCodes.Ldflda &&
                address.Operand is FieldReference f && f.Name == fields[n];
            patched &= receiver.OpCode == OpCodes.Nop && address.OpCode == OpCodes.Ldloca &&
                address.Operand == method.Body.Variables[n];
        }
        if (patched) return false;
        if (!original) throw new InvalidOperationException("Unrecognized damage reducer arguments; refusing to patch.");
        for (int n = 0; n < 2; n++)
        {
            // The callback must write per-modifier locals. Passing the accumulated
            // toughness here lets assigning modifiers erase it and apply buffs twice.
            code[index - 4 + n * 2].OpCode = OpCodes.Nop;
            code[index - 4 + n * 2].Operand = null;
            code[index - 3 + n * 2].OpCode = OpCodes.Ldloca;
            code[index - 3 + n * 2].Operand = method.Body.Variables[n];
        }
        Console.WriteLine("Fixed GiveDamage modifier accumulation (preserves toughness).");
        return true;
    }

    static void InstallCombat(string clientRoot, bool stageOnly)
    {
        var dll = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed", "Assembly-CSharp.dll"));
        var staged = dll + ".combat-staged";
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(dll)!);
        using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver }))
        {
            PatchDamageReduction(assembly.MainModule);
            PatchLegacyCombat(assembly.MainModule);
            if (PatchDamageReduction(assembly.MainModule)) throw new Exception("Combat patch is not idempotent.");
            if (PatchLegacyCombat(assembly.MainModule)) throw new Exception("Legacy combat patch is not idempotent.");
            assembly.Write(staged);
        }
        if (!stageOnly)
        {
            File.Copy(dll, dll + ".before-combat-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff"));
            File.Move(staged, dll, true);
        }
        Console.WriteLine(stageOnly ? $"Staged: {staged}" : "Installed damage reducer fix.");
    }

    static void TestCombat(string dll)
    {
        dll = Path.GetFullPath(dll);
        AssemblyLoadContext.Default.Resolving += (context, name) =>
        {
            var path = Path.Combine(Path.GetDirectoryName(dll)!, name.Name + ".dll");
            return File.Exists(path) ? context.LoadFromAssemblyPath(path) : null;
        };
        var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);
        var type = assembly.GetType("NShared.StatController", true)!.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
            .Single(t => t.GetField("multedDamagePower") != null && t.GetField("addedDamagePower") != null &&
                t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Any(m => m.Name.StartsWith("<GiveDamage>")));
        var callback = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Single(m => m.Name.StartsWith("<GiveDamage>"));
        var proxy = DispatchProxy.Create(assembly.GetType("NShared.IDamageAmplifier", true)!, typeof(CombatTestAmplifier));
        int cases = 0;
        foreach (bool assigning in new[] { true, false })
        foreach (int baseline in new[] { 0, -500 })
        foreach (var modifiers in new[] { Array.Empty<int>(), new[] { 0 }, new[] { 200 }, new[] { -200 },
                     new[] { 10 }, new[] { -200, -100 }, new[] { 200, -100 }, new[] { -100, 200 } })
        {
            var closure = Activator.CreateInstance(type, true)!;
            foreach (var field in type.GetFields().Where(f => f.FieldType.Name.StartsWith("<>c__DisplayClass")))
                field.SetValue(closure, Activator.CreateInstance(field.FieldType, true));
            type.GetField("multedDamagePower")!.SetValue(closure, baseline);
            type.GetField("addedDamagePower")!.SetValue(closure, -7);
            foreach (int amount in modifiers)
            {
                ((CombatTestAmplifier)proxy).Amount = amount;
                ((CombatTestAmplifier)proxy).Assigning = assigning;
                callback.Invoke(closure, new[] { proxy });
            }
            int expected = baseline + modifiers.Where(x => x < 0).Sum();
            int actual = (int)type.GetField("multedDamagePower")!.GetValue(closure)!;
            if (actual != expected || (int)type.GetField("addedDamagePower")!.GetValue(closure)! != -7)
                throw new Exception($"Reducer failed: baseline={baseline}, modifiers=[{string.Join(',', modifiers)}], expected={expected}, actual={actual}");
            cases++;
        }
        using var definition = AssemblyDefinition.ReadAssembly(dll);
        if (PatchDamageReduction(definition.MainModule)) throw new Exception("Test DLL was not already patched.");
        Console.WriteLine($"PASS: {cases} native reducer cases; patch reapplication makes no changes.");
        TestLegacyCombat(assembly, definition.MainModule);
        TestTechnomagicMechanics(assembly);
    }
}

public class CombatTestAmplifier : DispatchProxy
{
    public int Amount;
    public bool Assigning;
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod?.Name != "AccumulateAmplifyDamage") throw new NotSupportedException(targetMethod?.Name);
        args![2] = Assigning ? Amount : (int)args[2]! + Amount;
        return true;
    }
}
