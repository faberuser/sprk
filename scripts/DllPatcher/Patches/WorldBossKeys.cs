using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    const string WorldBossKeyGetter = "NGame2.NUI.NWindow.LobbyTopmostMenu.ICustomStaminaObserver.get_StaminaType";

    static int EnumValue(ModuleDefinition module, string type, string name) =>
        Convert.ToInt32(module.GetType(type).Fields.Single(f => f.Name == name).Constant);

    static void PatchWorldBossKeys(ModuleDefinition module)
    {
        var observer = module.GetType("NGame2.NUI.NWindow.LobbyTopmostMenu/ICustomStaminaObserver");
        var contract = observer.Methods.Single(m => m.Name == "get_StaminaType");
        var ticket = EnumValue(module, "NShared.StaminaType", "WorldBossGlobalTicket");
        var worldBoss = EnumValue(module, "NShared.BattleType", "WorldBoss");
        foreach (var name in new[] { "WorldBossGlobalRanking", "BattlePartySetting" })
        {
            var type = module.GetType("NGame2.NUI.NWindow." + name);
            if (!type.Interfaces.Any(i => i.InterfaceType.FullName == observer.FullName))
                type.Interfaces.Add(new InterfaceImplementation(observer));
            var getter = type.Methods.SingleOrDefault(m => m.Name == WorldBossKeyGetter);
            if (getter == null)
            {
                getter = new MethodDefinition(WorldBossKeyGetter,
                    MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.Virtual |
                    MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName,
                    module.GetType("NShared.StaminaType"));
                getter.Overrides.Add(contract);
                type.Methods.Add(getter);
            }
            getter.Body = new MethodBody(getter);
            var il = getter.Body.GetILProcessor();
            if (name == "BattlePartySetting")
            {
                // Only the top-bar observer changes. Preserve the provider's cost,
                // entry validation, start-button UI and other battle types.
                var fallback = il.Create(OpCodes.Ldarg_0);
                il.Append(il.Create(OpCodes.Ldarg_0));
                il.Append(il.Create(OpCodes.Callvirt, type.Methods.Single(m => m.Name == "get_BattleType")));
                il.Append(il.Create(OpCodes.Ldc_I4, worldBoss));
                il.Append(il.Create(OpCodes.Bne_Un, fallback));
                il.Append(il.Create(OpCodes.Ldc_I4, ticket));
                il.Append(il.Create(OpCodes.Ret));
                il.Append(fallback);
                il.Append(il.Create(OpCodes.Callvirt, type.Methods.Single(m => m.Name == "get_StaminaType")));
            }
            else il.Append(il.Create(OpCodes.Ldc_I4, ticket));
            il.Append(il.Create(OpCodes.Ret));
        }
        PatchWorldBossKeyMaximum(module);
        VerifyWorldBossKeys(module);
    }

    static void PatchWorldBossKeyMaximum(ModuleDefinition module)
    {
        var manager = module.GetType("NGame2.NAccount.UserStaminaManager");
        var adjust = manager.Methods.SingleOrDefault(m => m.Name == "SprkWorldBossKeyMaximum");
        if (adjust == null)
        {
            adjust = new MethodDefinition("SprkWorldBossKeyMaximum", MethodAttributes.Private | MethodAttributes.Static,
                module.TypeSystem.Int32);
            adjust.Parameters.Add(new ParameterDefinition("maximum", ParameterAttributes.None, module.TypeSystem.Int32));
            adjust.Parameters.Add(new ParameterDefinition("type", ParameterAttributes.None, module.GetType("NShared.StaminaType")));
            manager.Methods.Add(adjust);
        }
        adjust.Body = new MethodBody(adjust);
        var il = adjust.Body.GetILProcessor();
        var fallback = il.Create(OpCodes.Ldarg_0);
        il.Append(il.Create(OpCodes.Ldarg_1));
        il.Append(il.Create(OpCodes.Ldc_I4, EnumValue(module, "NShared.StaminaType", "WorldBossGlobalTicket")));
        il.Append(il.Create(OpCodes.Bne_Un, fallback));
        il.Append(il.Create(OpCodes.Ldc_I4_2));
        il.Append(il.Create(OpCodes.Ret));
        il.Append(fallback);
        il.Append(il.Create(OpCodes.Ret));

        var update = manager.Methods.Single(m => m.Name == "UpdateStaminaValue");
        if (!update.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == adjust.Name))
        {
            var store = update.Body.Instructions.Single(i => i.OpCode == OpCodes.Stfld && i.Operand is FieldReference f && f.Name == "MaxValue");
            // Keep the native calculation for every other resource. Changing the
            // cached maximum also keeps tooltips/recovery checks consistent.
            update.Body.GetILProcessor().InsertBefore(store, Instruction.Create(OpCodes.Ldarg_1));
            update.Body.GetILProcessor().InsertBefore(store, Instruction.Create(OpCodes.Call, adjust));
        }
    }

    static void VerifyWorldBossKeys(ModuleDefinition module)
    {
        var ticket = EnumValue(module, "NShared.StaminaType", "WorldBossGlobalTicket");
        foreach (var name in new[] { "WorldBossGlobalRanking", "BattlePartySetting" })
        {
            var type = module.GetType("NGame2.NUI.NWindow." + name);
            var getter = type.Methods.Single(m => m.Name == WorldBossKeyGetter);
            if (!getter.Overrides.Any(m => m.DeclaringType.Name == "ICustomStaminaObserver") ||
                !type.Interfaces.Any(i => i.InterfaceType.Name == "ICustomStaminaObserver") ||
                !getter.Body.Instructions.Any(i => i.OpCode == OpCodes.Ldc_I4 && Equals(i.Operand, ticket)))
                throw new InvalidOperationException("World Boss key observer missing: " + name);
            if (name == "BattlePartySetting" &&
                (!getter.Body.Instructions.Any(i => i.OpCode == OpCodes.Bne_Un) ||
                 !getter.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "get_StaminaType")))
                throw new InvalidOperationException("Other battle resource displays must retain their native getter.");
        }
        var manager = module.GetType("NGame2.NAccount.UserStaminaManager");
        var adjust = manager.Methods.Single(m => m.Name == "SprkWorldBossKeyMaximum");
        var instructions = adjust.Body.Instructions;
        var expected = new[] { OpCodes.Ldarg_1, OpCodes.Ldc_I4, OpCodes.Bne_Un, OpCodes.Ldc_I4_2, OpCodes.Ret, OpCodes.Ldarg_0, OpCodes.Ret };
        if (!instructions.Select(i => i.OpCode).SequenceEqual(expected) ||
            !Equals(instructions[1].Operand, ticket) || !ReferenceEquals(instructions[2].Operand, instructions[5]))
            throw new InvalidOperationException("World Boss maximum must be two, with an unchanged fallback for other resources.");
        var call = manager.Methods.Single(m => m.Name == "UpdateStaminaValue").Body.Instructions
            .Single(i => i.Operand is MethodReference m && m.Name == adjust.Name);
        if (call.Previous.OpCode != OpCodes.Ldarg_1 || call.Next.OpCode != OpCodes.Stfld ||
            call.Next.Operand is not FieldReference { Name: "MaxValue" })
            throw new InvalidOperationException("World Boss maximum hook must update the native cached maximum.");
        Console.WriteLine("Verified World Boss key observers and maximum of two, preserving other resource balances and limits.");
    }

    static void VerifyWorldBossKeysFile(string root)
    {
        using var game = AssemblyDefinition.ReadAssembly(Path.Combine(root, "King's Raid_Data", "Managed", "Assembly-CSharp.dll"));
        VerifyWorldBossKeys(game.MainModule);
    }

    static void InstallWorldBossKeys(string root, bool stageOnly)
    {
        var dll = Path.GetFullPath(Path.Combine(root, "King's Raid_Data", "Managed", "Assembly-CSharp.dll"));
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(dll)!);
        using var game = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { InMemory = true, AssemblyResolver = resolver });
        PatchWorldBossKeys(game.MainModule);
        var staged = dll + ".world-boss-keys-staged";
        game.Write(staged);
        using (var check = AssemblyDefinition.ReadAssembly(staged)) VerifyWorldBossKeys(check.MainModule);
        if (stageOnly) return;
        if (System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, root)))
            throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
        File.Copy(dll, dll + ".before-world-boss-keys-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
        File.Move(staged, dll, true);
        VerifyWorldBossKeysFile(root);
    }
}
