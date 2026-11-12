using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    const string PartyJoinLockedMessage = "You haven't unlock this raid or stage yet.";
    static readonly string[] PartyJoinResultTypes = {
        "NShared.NMessage.NProtocol.JoinPartyRes/ResultType",
        "NShared.JoinRaidRoom.ResultType",
        "NShared.JoinPartyDungeonRoom.ResultType"
    };

    static void PatchPartyJoinUnlock(ModuleDefinition module)
    {
        foreach (var name in PartyJoinResultTypes) {
            var type = module.GetType(name) ?? throw new InvalidOperationException("Missing join result enum: " + name);
            if (type.Fields.Any(f => f.Name == "DungeonLocked")) continue;
            var next = type.Fields.Where(f => f.HasConstant).Max(f => Convert.ToInt32(f.Constant)) + 1;
            type.Fields.Add(new FieldDefinition("DungeonLocked", FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault, type) { Constant = next });
        }
        var text = module.GetType("NVespa.NGlobal.LocalizationManager").Methods.Single(m =>
            m.Name == "GetString" && m.Parameters.Count == 2 && m.Parameters[0].ParameterType.FullName == "System.Enum");
        if (text.Body.Instructions.Take(9).Any(i => i.OpCode == OpCodes.Ldstr && Equals(i.Operand, "DungeonLocked"))) {
            var prefix = text.Body.Instructions.Take(9).ToArray();
            if (prefix.Length != 9 || prefix[1].OpCode != OpCodes.Brfalse || prefix[6].Operand != prefix[1].Operand ||
                prefix[7].OpCode != OpCodes.Ldstr || prefix[8].OpCode != OpCodes.Ret)
                throw new InvalidOperationException("Unexpected installed join message hook.");
            prefix[7].Operand = PartyJoinLockedMessage;
        } else {
            var il = text.Body.GetILProcessor();
            var first = text.Body.Instructions[0];
            var toString = new MethodReference("ToString", module.TypeSystem.String, module.TypeSystem.Object) { HasThis = true };
            var equals = new MethodReference("op_Equality", module.TypeSystem.Boolean, module.TypeSystem.String);
            equals.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));
            equals.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));
            foreach (var instruction in new[] {
                Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Brfalse, first),
                Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Callvirt, toString),
                Instruction.Create(OpCodes.Ldstr, "DungeonLocked"), Instruction.Create(OpCodes.Call, equals),
                Instruction.Create(OpCodes.Brfalse, first), Instruction.Create(OpCodes.Ldstr, PartyJoinLockedMessage),
                Instruction.Create(OpCodes.Ret)
            }) il.InsertBefore(first, instruction);
            text.Body.MaxStackSize = Math.Max(2, text.Body.MaxStackSize);
        }
        VerifyPartyJoinUnlock(module);
    }

    static void VerifyPartyJoinUnlock(ModuleDefinition module)
    {
        foreach (var name in PartyJoinResultTypes) {
            var type = module.GetType(name);
            var locked = type.Fields.Single(f => f.Name == "DungeonLocked");
            if (!locked.HasConstant || type.Fields.Where(f => f.HasConstant).GroupBy(f => Convert.ToInt32(f.Constant)).Any(g => g.Count() > 1))
                throw new InvalidOperationException("Invalid join enum extension: " + name);
        }
        var text = module.GetType("NVespa.NGlobal.LocalizationManager").Methods.Single(m =>
            m.Name == "GetString" && m.Parameters.Count == 2 && m.Parameters[0].ParameterType.FullName == "System.Enum");
        var prefix = text.Body.Instructions.Take(9).ToArray();
        if (prefix.Length != 9 || prefix[1].OpCode != OpCodes.Brfalse || prefix[6].Operand != prefix[1].Operand ||
            !Equals(prefix[4].Operand, "DungeonLocked") || !Equals(prefix[7].Operand, PartyJoinLockedMessage) || prefix[8].OpCode != OpCodes.Ret)
            throw new InvalidOperationException("Locked raid result does not use the unlock explanation and preserve native fallback.");
    }

    static void InstallPartyJoinUnlock(string clientRoot, bool stageOnly)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var staged = dll + ".party-join-unlock-staged";
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver })) {
            PatchPartyJoinUnlock(assembly.MainModule);
            assembly.Write(staged);
        }
        using (var verify = AssemblyDefinition.ReadAssembly(staged)) VerifyPartyJoinUnlock(verify.MainModule);
        if (!stageOnly) {
            var backup = Path.Combine("target", "party-join-unlock-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup);
            File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
            File.Move(staged, dll, true);
        }
        Console.WriteLine(stageOnly ? "Staged and verified locked raid join message." : "Installed and verified locked raid join message. Restart the client.");
    }

    static void VerifyPartyJoinUnlockFile(string clientRoot)
    {
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(clientRoot, "King's Raid_Data", "Managed", "Assembly-CSharp.dll"));
        VerifyPartyJoinUnlock(assembly.MainModule);
        if (assembly.MainModule.AssemblyReferences.Any(r => r.Name == "SprkStoryAuto")) VerifyStoryAuto(assembly.MainModule);
        Console.WriteLine("Verified chat, dragon and story join results and unlock message.");
    }

    static void TestPartyJoinUnlock()
    {
        using var assembly = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition("PartyJoinUnlockTest", new Version(1,0)), "PartyJoinUnlockTest", ModuleKind.Dll);
        var module = assembly.MainModule;
        var enumBase = new TypeReference("System", "Enum", module, module.TypeSystem.CoreLibrary);
        foreach (var name in PartyJoinResultTypes) {
            var outerName = name.Split('/')[0];
            var split = outerName.LastIndexOf('.');
            var type = new TypeDefinition(outerName[..split], outerName[(split+1)..], TypeAttributes.Public, module.TypeSystem.Object);
            module.Types.Add(type);
            if (name.Contains('/')) {
                var nested = new TypeDefinition("", "ResultType", TypeAttributes.NestedPublic | TypeAttributes.Sealed, enumBase);
                type.NestedTypes.Add(nested);type = nested;
            } else {type.BaseType = enumBase;type.Attributes |= TypeAttributes.Sealed;}
            type.Fields.Add(new FieldDefinition("value__", FieldAttributes.Public | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName, module.TypeSystem.Int32));
            foreach (var (label,value) in new[] { ("Fail",0), ("Success",1), ("InvalidParty",2) })
                type.Fields.Add(new FieldDefinition(label, FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault, type) { Constant = value });
        }
        var localizer = new TypeDefinition("NVespa.NGlobal", "LocalizationManager", TypeAttributes.Public, module.TypeSystem.Object);
        module.Types.Add(localizer);
        var text = new MethodDefinition("GetString", MethodAttributes.Public | MethodAttributes.Static, module.TypeSystem.String);
        text.Parameters.Add(new ParameterDefinition(enumBase));
        text.Parameters.Add(new ParameterDefinition(new ArrayType(module.TypeSystem.Object)));
        localizer.Methods.Add(text);
        text.Body.Instructions.Add(Instruction.Create(OpCodes.Ldstr, "native fallback"));
        text.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        PatchPartyJoinUnlock(module);var count = text.Body.Instructions.Count;PatchPartyJoinUnlock(module);
        if (text.Body.Instructions.Count != count) throw new InvalidOperationException("Patch duplicated the message hook.");
        using var bytes = new MemoryStream();assembly.Write(bytes);
        var runtime = System.Reflection.Assembly.Load(bytes.ToArray());
        var get = runtime.GetType(localizer.FullName)!.GetMethod("GetString")!;
        foreach (var name in PartyJoinResultTypes) {
            var type = runtime.GetType(name.Replace('/', '+'))!;
            foreach (var label in new[] { "DungeonLocked", "InvalidParty", "Success", "Fail" }) {
                var value = Enum.Parse(type, label);
                var actual = (string)get.Invoke(null, new object[] {value, Array.Empty<object>()})!;
                if (actual != (label == "DungeonLocked" ? PartyJoinLockedMessage : "native fallback"))
                    throw new InvalidOperationException("Wrong join message for " + name + "/" + label);
            }
        }
        if ((string)get.Invoke(null, new object?[] {null, Array.Empty<object>()})! != "native fallback")
            throw new InvalidOperationException("Null enum did not preserve native behavior.");
        Console.WriteLine("PASS: production join message IL handles all three enums, preserves other errors/nulls, and patches idempotently.");
    }
}
