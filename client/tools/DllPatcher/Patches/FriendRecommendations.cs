using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    const string FriendRecommendType = "NGame2.NUI.NComponent2.FriendRecommendPanel";

    static MethodDefinition FriendRecommendationCallback(ModuleDefinition module) => module.GetType(FriendRecommendType)
        .Methods.Single(m => m.HasBody && m.Name.Contains("<RefreshRecommendFriendList>")
            && m.Body.Instructions.Any(i => i.Operand is MethodReference r && r.Name == "RemoveAll"));

    static void PatchFriendRecommendations(ModuleDefinition module)
    {
        var panel = module.GetType(FriendRecommendType);
        var filter = panel.Methods.Single(m => m.Name == "RemoveCondition");
        var info = module.GetType("NShared.CachedUserInfo");
        var empty = (MethodReference)filter.Body.Instructions.Single(i => i.Operand is MethodReference r
            && r.DeclaringType.FullName == "System.String" && r.Name == "IsNullOrEmpty").Operand;
        // Keep validity checks, but never reject players because of presence or
        // last logout (including missing/legacy timestamps). The server samples
        // the full eligible population before applying its result limit.
        filter.Body.Instructions.Clear();
        filter.Body.ExceptionHandlers.Clear();
        filter.Body.Variables.Clear();
        filter.Body.InitLocals = false;
        var il = filter.Body.GetILProcessor();
        var reject = Instruction.Create(OpCodes.Ldc_I4_1);
        il.Append(Instruction.Create(OpCodes.Ldarg_1));
        il.Append(Instruction.Create(OpCodes.Brfalse, reject));
        il.Append(Instruction.Create(OpCodes.Ldarg_1));
        il.Append(Instruction.Create(OpCodes.Callvirt, info.Methods.Single(m => m.Name == "get_TeamLevel")));
        il.Append(Instruction.Create(OpCodes.Brfalse, reject));
        il.Append(Instruction.Create(OpCodes.Ldarg_1));
        il.Append(Instruction.Create(OpCodes.Callvirt, info.Methods.Single(m => m.Name == "get_Nick")));
        il.Append(Instruction.Create(OpCodes.Call, empty));
        il.Append(Instruction.Create(OpCodes.Ret));
        il.Append(reject);
        il.Append(Instruction.Create(OpCodes.Ret));

        var callback = FriendRecommendationCallback(module);
        foreach (var sort in callback.Body.Instructions.Where(i => i.Operand is MethodReference r
            && r.DeclaringType.FullName == "System.Array" && r.Name == "Sort").ToArray())
        {
            // Consume the array and comparison delegate without reordering the
            // server's random sample. Retain all native request/grid callbacks.
            sort.OpCode = OpCodes.Pop;
            sort.Operand = null;
            callback.Body.GetILProcessor().InsertAfter(sort, Instruction.Create(OpCodes.Pop));
        }

        var open = panel.Methods.Single(m => m.Name == "Open");
        var cache = panel.Fields.Single(f => f.Name == "_friendInfos");
        if (open.Body.Instructions[2].OpCode != OpCodes.Stfld || open.Body.Instructions[2].Operand != cache)
        {
            var first = open.Body.Instructions[0];
            foreach (var instruction in new[] { Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldnull), Instruction.Create(OpCodes.Stfld, cache) })
                open.Body.GetILProcessor().InsertBefore(first, instruction);
        }
        VerifyFriendRecommendations(module);
    }

    static void VerifyFriendRecommendations(ModuleDefinition module)
    {
        var panel = module.GetType(FriendRecommendType);
        var filter = panel.Methods.Single(m => m.Name == "RemoveCondition");
        var calls = filter.Body.Instructions.Select(i => i.Operand).OfType<MethodReference>().Select(m => m.Name).ToArray();
        if (!calls.SequenceEqual(new[] { "get_TeamLevel", "get_Nick", "IsNullOrEmpty" }))
            throw new InvalidOperationException("Recommendations must check profile validity without an inactivity cutoff.");
        var open = panel.Methods.Single(m => m.Name == "Open");
        var prefix = open.Body.Instructions.Take(3).ToArray();
        if (prefix[0].OpCode != OpCodes.Ldarg_0 || prefix[1].OpCode != OpCodes.Ldnull
            || prefix[2].OpCode != OpCodes.Stfld || prefix[2].Operand is not FieldReference f || f.Name != "_friendInfos"
            || !open.Body.Instructions.Any(i => i.Operand is MethodReference r && r.Name == "RefreshRecommendFriendList"))
            throw new InvalidOperationException("Opening recommendations must fetch a fresh random sample.");
        var callback = FriendRecommendationCallback(module);
        if (callback.Body.Instructions.Any(i => i.Operand is MethodReference r && r.Name == "Sort")
            || !callback.Body.Instructions.Any(i => i.Operand is MethodReference r && r.Name == "SetupFriendSlotComponent"))
            throw new InvalidOperationException("Recommendation rendering must preserve server order.");
        var setup = panel.Methods.Single(m => m.Name == "SetupFriendSlotComponent");
        foreach (var name in new[] { "IsInvitee", "IsInvitor" })
            if (!setup.Body.Instructions.Any(i => i.Operand is MethodReference r && r.Name == name))
                throw new InvalidOperationException("Missing native friend invitation exclusion: " + name);
        foreach (var method in new[] { filter, open, callback }) VerifyArenaResultStack(method);
    }

    static void VerifyFriendRecommendationsFile(string root, bool staged)
    {
        var managed = Path.GetFullPath(Path.Combine(root, "King's Raid_Data", "Managed"));
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(managed, "Assembly-CSharp.dll"
            + (staged ? ".friend-recommendations-staged" : "")), new ReaderParameters { AssemblyResolver = resolver });
        VerifyFriendRecommendations(assembly.MainModule);
        Console.WriteLine("Verified full-population recommendations, random order, fresh opening, invitation guards and IL stack.");
    }

    static void InstallFriendRecommendations(string root, bool stageOnly)
    {
        var managed = Path.GetFullPath(Path.Combine(root, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var staged = dll + ".friend-recommendations-staged";
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver }))
        {
            PatchFriendRecommendations(assembly.MainModule);
            var first = new MemoryStream();
            assembly.Write(first);
            PatchFriendRecommendations(assembly.MainModule);
            var second = new MemoryStream();
            assembly.Write(second);
            if (!first.ToArray().SequenceEqual(second.ToArray()))
                throw new InvalidOperationException("Friend recommendation patch must be idempotent.");
            File.WriteAllBytes(staged, second.ToArray());
        }
        VerifyFriendRecommendationsFile(root, true);
        if (!stageOnly)
        {
            // Never install/publish while the game is using the old DLL.
            if (System.Diagnostics.Process.GetProcessesByName("King's Raid").Any())
                throw new InvalidOperationException("Fix staged and verified. Close King's Raid before installation.");
            var backup = Path.Combine("target", "friend-recommendations-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup);
            File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
            File.Move(staged, dll, true);
            VerifyFriendRecommendationsFile(root, false);
        }
        Console.WriteLine(stageOnly ? "Staged friend recommendations fix." : "Installed friend recommendations fix.");
    }

    static void TestFriendRecommendations(string root)
    {
        VerifyFriendRecommendationsFile(root, false);
        var managed = Path.GetFullPath(Path.Combine(root, "King's Raid_Data", "Managed"));
        System.Runtime.Loader.AssemblyLoadContext.Default.Resolving += (context, name) => {
            var path = Path.Combine(managed, name.Name + ".dll");
            return File.Exists(path) ? context.LoadFromAssemblyPath(path) : null;
        };
        var assembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(managed, "Assembly-CSharp.dll"));
        var profile = assembly.GetType("NShared.CachedUserInfo", true)!;
        var filter = assembly.GetType(FriendRecommendType, true)!.GetMethod("RemoveCondition",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        // This predicate never uses its MonoBehaviour instance. Call it directly
        // without starting Unity or constructing a native Unity component.
        var probe = new System.Reflection.Emit.DynamicMethod("RecommendationFilterProbe", typeof(bool), new[] { typeof(object) }, true);
        var il = probe.GetILGenerator();
        il.Emit(System.Reflection.Emit.OpCodes.Ldnull);
        il.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
        il.Emit(System.Reflection.Emit.OpCodes.Castclass, profile);
        il.Emit(System.Reflection.Emit.OpCodes.Call, filter);
        il.Emit(System.Reflection.Emit.OpCodes.Ret);
        var removed = (Func<object?, bool>)probe.CreateDelegate(typeof(Func<object?, bool>));
        int cases = 0;
        foreach (bool online in new[] { false, true })
        foreach (string? logout in new[] { null, "", "invalid", "2000-01-01 00:00:00", "2026-10-18 00:00:00" })
        foreach (int level in new[] { 0, 1, 100 })
        foreach (string? nick in new[] { null, "", "Player" })
        {
            var player = Activator.CreateInstance(profile)!;
            foreach (var (name, value) in new (string, object?)[] { ("IsOnline", online), ("LastLogoutTime", logout),
                ("TeamLevel", level), ("Nick", nick) })
                profile.GetProperty(name)!.SetValue(player, value);
            if (removed(player) != (level == 0 || string.IsNullOrEmpty(nick)))
                throw new InvalidOperationException("Recommendation eligibility depends on presence/logout time or accepts an invalid profile.");
            cases++;
        }
        if (!removed(null)) throw new InvalidOperationException("Null recommendations must be ignored.");
        Console.WriteLine($"PASS: {cases + 1} installed predicate cases, including old/missing/invalid logout timestamps and offline players.");
    }
}
