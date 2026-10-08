using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Text.Json;

namespace DllPatcher;

partial class Program
{
    // These components are deleted, including their nested types. Do not retain SDK stubs.
    internal static bool IsTelemetryAssembly(string name) =>
        name.StartsWith("Firebase.", StringComparison.Ordinal) ||
        name.StartsWith("Unity.Services.", StringComparison.Ordinal) ||
        name is "UnityEngine.UnityAnalyticsModule" or "UnityEngine.UnityAnalyticsCommonModule" or
            "UnityEngine.PerformanceReportingModule" or "UnityEngine.CrashReportingModule" or
            "UnityEngine.UnityConnectModule";

    internal static bool IsTelemetryType(TypeReference? type)
    {
        if (type == null) return false;
        if (type is GenericInstanceType generic && generic.GenericArguments.Any(IsTelemetryType)) return true;
        if (type is TypeSpecification spec) return IsTelemetryType(spec.ElementType);
        var name = type.FullName;
        return (type.Scope is AssemblyNameReference scope && IsTelemetryAssembly(scope.Name)) ||
            name.StartsWith("Firebase.", StringComparison.Ordinal) ||
            name.StartsWith("Unity.Services.", StringComparison.Ordinal) ||
            name.StartsWith("com.adjust.sdk.", StringComparison.Ordinal) ||
            name.StartsWith("MasangAnalytics.", StringComparison.Ordinal) ||
            name.StartsWith("NGame2.NAnalytics.", StringComparison.Ordinal) ||
            name.Split('/')[0] is "GaHiddenWebView" or "WebStepLogWrapper" or
                "NGame2.AnalyticsManager" or "NGame2.NConfig.ClientDebugAnalytics" ||
            name.StartsWith("UnityEngine.Analytics.", StringComparison.Ordinal) ||
            name is "UnityEngine.CrashReport" or "UnityEngine.CrashReportHandler.CrashReportHandler" or
                "UnityEngine.Connect.UnityConnectSettings" or "UnityEngine.Advertisements.UnityAdsSettings" ||
            name.StartsWith("UnityEngine.Purchasing.Telemetry.", StringComparison.Ordinal) ||
            (name.StartsWith("UnityEngine.Purchasing.", StringComparison.Ordinal) &&
                (type.Name.Contains("Analytics", StringComparison.Ordinal) ||
                 type.Name is "UnityServicesInitializationChecker" or "IUnityServicesInitializationChecker" ||
                 name.StartsWith("UnityEngine.Purchasing.Registration.IapCoreInitializeCallback", StringComparison.Ordinal)));
    }

    static bool IsTelemetryMethod(MethodReference method) => IsTelemetryType(method.DeclaringType) ||
        (method is GenericInstanceMethod generic && generic.GenericArguments.Any(IsTelemetryType)) ||
        method.Name is "InitializeUnityServicesAsync" or "AutoInitializeUnityGamingServicesIfEnabled" or
            "ShouldAutoInitUgs" or "SendAdjustEvent" ||
        ((method.DeclaringType.Namespace ?? "").StartsWith("UnityEngine.Purchasing", StringComparison.Ordinal) &&
            (method.Name.Contains("Analytics", StringComparison.Ordinal) ||
             method.Name.Contains("telemetry", StringComparison.OrdinalIgnoreCase) ||
             method.Name == "SendTransactionEvent"));

    static void ReplaceInstruction(MethodDefinition method, Instruction old, IEnumerable<Instruction> replacement)
    {
        // Keep the instruction identity so branch targets and exception boundaries remain valid.
        var items = replacement.ToArray();
        old.OpCode = items.Length == 0 ? OpCodes.Nop : items[0].OpCode;
        old.Operand = items.Length == 0 ? null : items[0].Operand;
        var il = method.Body.GetILProcessor();
        var cursor = old;
        foreach (var next in items.Skip(1)) { il.InsertAfter(cursor, next); cursor = next; }
    }

    static IEnumerable<Instruction> DefaultValue(ModuleDefinition module, TypeReference type)
    {
        if (type.MetadataType == MetadataType.Void) yield break;
        if (type.FullName == "System.Threading.Tasks.Task")
        {
            var task = new TypeReference("System.Threading.Tasks", "Task", module, module.TypeSystem.CoreLibrary);
            yield return Instruction.Create(OpCodes.Call, new MethodReference("get_CompletedTask", task, task));
        }
        else if (type.MetadataType == MetadataType.String) yield return Instruction.Create(OpCodes.Ldstr, "");
        else if (IsTelemetryType(type) || !type.IsValueType) yield return Instruction.Create(OpCodes.Ldnull);
        else if (type.MetadataType is MetadataType.Int64 or MetadataType.UInt64)
        {
            yield return Instruction.Create(OpCodes.Ldc_I4_0);
            yield return Instruction.Create(OpCodes.Conv_I8);
        }
        else if (type.MetadataType == MetadataType.Single) yield return Instruction.Create(OpCodes.Ldc_R4, 0f);
        else if (type.MetadataType == MetadataType.Double) yield return Instruction.Create(OpCodes.Ldc_R8, 0d);
        else yield return Instruction.Create(OpCodes.Ldc_I4_0);
    }

    static TypeReference EraseTelemetrySignature(ModuleDefinition module, TypeReference type)
    {
        if (IsTelemetryType(type)) return module.TypeSystem.Object;
        return type;
    }

    static void RemoveTelemetryFromModule(ModuleDefinition module)
    {
        var removedMethods = module.GetTypes().Where(t => !IsTelemetryType(t))
            .SelectMany(t => t.Methods).Where(IsTelemetryMethod).ToHashSet();
        // Delete the async state machines belonging to deleted initialization methods too.
        var removedNested = module.GetTypes().Where(t =>
            t.Name.StartsWith("<InitializeUnityServicesAsync>", StringComparison.Ordinal)).ToHashSet();
        var removedFields = module.GetTypes().SelectMany(t => t.Fields)
            .Where(f => IsTelemetryType(f.FieldType)).Select(f => f.FullName).ToHashSet();

        foreach (var type in module.GetTypes().Where(t => !IsTelemetryType(t) && !removedNested.Contains(t)).ToArray())
        {
            foreach (var method in type.Methods.Where(m => !removedMethods.Contains(m)).ToArray())
            {
                if (!method.HasBody) continue;
                if (!method.Body.Variables.Any(v => IsTelemetryType(v.VariableType)) &&
                    !method.Body.Instructions.Any(i => i.Operand switch
                    {
                        MethodReference call => IsTelemetryMethod(call) || call.Parameters.Any(p => IsTelemetryType(p.ParameterType)),
                        FieldReference field => IsTelemetryType(field.DeclaringType) || IsTelemetryType(field.FieldType),
                        TypeReference operand => IsTelemetryType(operand),
                        _ => false
                    })) continue;
                // The analytics manager was also registered as a gameplay observer. Delete that registration.
                foreach (var factory in method.Body.Instructions.Where(i => i.Operand is MethodReference call &&
                    call.Name == "NewInstance" && IsTelemetryType(call.DeclaringType)).ToArray())
                {
                    if (factory.Next?.Operand is MethodReference registration && registration.Name == "AddUserObserver" &&
                        factory.Previous?.OpCode == OpCodes.Ldarg_0)
                    {
                        foreach (var old in new[] { factory.Previous, factory, factory.Next })
                        { old.OpCode = OpCodes.Nop; old.Operand = null; }
                    }
                }
                // Expanding pop sequences can exceed short branch ranges.
                foreach (var instruction in method.Body.Instructions)
                {
                    if (instruction.OpCode.OperandType == OperandType.ShortInlineBrTarget)
                    {
                        var longCode = typeof(OpCodes).GetFields().Select(f => f.GetValue(null))
                            .OfType<OpCode>().Single(c => c.Name == instruction.OpCode.Name[..^2]);
                        instruction.OpCode = longCode;
                    }
                }
                foreach (var instruction in method.Body.Instructions.ToArray())
                {
                    if (instruction.Operand is MethodReference call && IsTelemetryMethod(call))
                    {
                        var replacement = new List<Instruction>();
                        if (call.Name == "ExecuteTimedAction")
                        {
                            // Keep the store action; remove the metric argument and service receiver.
                            replacement.Add(Instruction.Create(OpCodes.Pop));
                            var actionType = new TypeReference("System", "Action", module, module.TypeSystem.CoreLibrary);
                            var action = new VariableDefinition(actionType);
                            method.Body.Variables.Add(action);
                            replacement.Add(Instruction.Create(OpCodes.Stloc, action));
                            replacement.Add(Instruction.Create(OpCodes.Pop));
                            replacement.Add(Instruction.Create(OpCodes.Ldloc, action));
                            replacement.Add(Instruction.Create(OpCodes.Callvirt,
                                new MethodReference("Invoke", module.TypeSystem.Void, actionType) { HasThis = true }));
                        }
                        else
                        {
                            var count = call.Parameters.Count +
                                (call.HasThis && instruction.OpCode != OpCodes.Newobj ? 1 : 0);
                            replacement.AddRange(Enumerable.Range(0, count).Select(_ => Instruction.Create(OpCodes.Pop)));
                            var result = instruction.OpCode == OpCodes.Newobj ? call.DeclaringType : call.ReturnType;
                            // Singleton getters use a generic return parameter; deleted components yield null.
                            if (result is GenericParameter && IsTelemetryType(call.DeclaringType))
                                replacement.Add(Instruction.Create(OpCodes.Ldnull));
                            else replacement.AddRange(DefaultValue(module, result));
                        }
                        if (instruction.OpCode is var opcode && (opcode == OpCodes.Ldftn || opcode == OpCodes.Ldvirtftn))
                            throw new InvalidOperationException($"Unexpected telemetry delegate in {method.FullName}");
                        ReplaceInstruction(method, instruction, replacement);
                    }
                    else if (instruction.Operand is FieldReference field &&
                        (IsTelemetryType(field.DeclaringType) || IsTelemetryType(field.FieldType) || removedFields.Contains(field.FullName)))
                    {
                        var replacement = new List<Instruction>();
                        if (instruction.OpCode == OpCodes.Ldfld) replacement.Add(Instruction.Create(OpCodes.Pop));
                        else if (instruction.OpCode == OpCodes.Stfld)
                            replacement.AddRange(new[] { Instruction.Create(OpCodes.Pop), Instruction.Create(OpCodes.Pop) });
                        else if (instruction.OpCode == OpCodes.Stsfld) replacement.Add(Instruction.Create(OpCodes.Pop));
                        else if (instruction.OpCode != OpCodes.Ldsfld)
                            throw new InvalidOperationException($"Unexpected telemetry field operation in {method.FullName}: {instruction}");
                        if (instruction.OpCode == OpCodes.Ldfld || instruction.OpCode == OpCodes.Ldsfld)
                            replacement.AddRange(DefaultValue(module, field.FieldType));
                        ReplaceInstruction(method, instruction, replacement);
                    }
                    else if (instruction.Operand is TypeReference operand && IsTelemetryType(operand))
                        throw new InvalidOperationException($"Unexpected telemetry type operation in {method.FullName}: {instruction}");
                    else if (instruction.OpCode == OpCodes.Ldstr && instruction.Operand is string text &&
                        (text.Contains("/api/analytics/", StringComparison.Ordinal) ||
                         text == "https://qa.masanggames.com")) instruction.Operand = "";
                }
                foreach (var variable in method.Body.Variables)
                    variable.VariableType = EraseTelemetrySignature(module, variable.VariableType);
                // Rewrite calls to retained store constructors after their telemetry-only parameter types are erased.
                foreach (var call in method.Body.Instructions.Select(i => i.Operand).OfType<MethodReference>())
                {
                    call.ReturnType = EraseTelemetrySignature(module, call.ReturnType);
                    foreach (var parameter in call.Parameters)
                    {
                        if (IsTelemetryType(parameter.ParameterType)) parameter.Name = "unused";
                        parameter.ParameterType = EraseTelemetrySignature(module, parameter.ParameterType);
                    }
                }
            }
            foreach (var method in type.Methods.Where(m => !removedMethods.Contains(m)))
            {
                method.ReturnType = EraseTelemetrySignature(module, method.ReturnType);
                foreach (var parameter in method.Parameters)
                {
                    if (IsTelemetryType(parameter.ParameterType)) parameter.Name = "unused";
                    parameter.ParameterType = EraseTelemetrySignature(module, parameter.ParameterType);
                }
            }
        }
        // Cecil call operands may be the actual definitions; delete only after all callers are rewritten.
        foreach (var type in module.GetTypes().Where(t => !IsTelemetryType(t) && !removedNested.Contains(t)).ToArray())
        {
            foreach (var field in type.Fields.Where(f => removedFields.Contains(f.FullName)).ToArray()) type.Fields.Remove(field);
            foreach (var property in type.Properties.Where(p => IsTelemetryType(p.PropertyType) ||
                (p.GetMethod != null && removedMethods.Contains(p.GetMethod)) ||
                (p.SetMethod != null && removedMethods.Contains(p.SetMethod))).ToArray()) type.Properties.Remove(property);
            foreach (var method in type.Methods.Where(m => removedMethods.Contains(m)).ToArray()) type.Methods.Remove(method);
            foreach (var nested in type.NestedTypes.Where(t => IsTelemetryType(t) || removedNested.Contains(t)).ToArray())
                type.NestedTypes.Remove(nested);
        }
        foreach (var type in module.Types.Where(IsTelemetryType).ToArray()) module.Types.Remove(type);
        foreach (var exported in module.ExportedTypes.Where(t =>
            t.Scope is AssemblyNameReference scope && IsTelemetryAssembly(scope.Name)).ToArray())
            module.ExportedTypes.Remove(exported);
        foreach (var reference in module.AssemblyReferences.Where(r => IsTelemetryAssembly(r.Name)).ToArray())
            module.AssemblyReferences.Remove(reference);
        Console.WriteLine($"Removed telemetry components from {module.Name}");
    }

    static void RemoveClientTelemetry(string clientRoot, bool stageOnly, string stageRoot)
    {
        var data = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data"));
        var managed = Path.Combine(data, "Managed");
        var stage = Path.GetFullPath(stageRoot);
        if (stage.StartsWith(data + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Telemetry staging must be outside the client data directory.");
        Directory.CreateDirectory(stage);
        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        var removedFiles = new List<string>();
        var modifiedFiles = new List<string>();
        var removedTypes = new List<string>();
        foreach (var file in Directory.GetFiles(managed, "*.dll"))
        {
            using var assembly = AssemblyDefinition.ReadAssembly(file, new ReaderParameters { AssemblyResolver = resolver });
            if (IsTelemetryAssembly(assembly.Name.Name))
            {
                removedFiles.Add(Path.GetRelativePath(data, file));
                if (File.Exists(Path.ChangeExtension(file, ".pdb"))) removedFiles.Add(Path.GetRelativePath(data, Path.ChangeExtension(file, ".pdb")));
                continue;
            }
            var module = assembly.MainModule;
            removedTypes.AddRange(module.GetTypes().Where(IsTelemetryType).Select(t => assembly.Name.Name + ":" + t.FullName));
            if (assembly.Name.Name != "Assembly-CSharp" &&
                !module.AssemblyReferences.Any(r => IsTelemetryAssembly(r.Name)) &&
                !module.GetTypes().Any(IsTelemetryType)) continue;
            RemoveTelemetryFromModule(module);
            var relative = Path.GetRelativePath(data, file);
            var output = Path.Combine(stage, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(output)!);
            assembly.Write(output);
            modifiedFiles.Add(relative);
            // Old symbol files contain removed code and no longer match the rewritten assembly.
            if (File.Exists(Path.ChangeExtension(file, ".pdb"))) removedFiles.Add(Path.GetRelativePath(data, Path.ChangeExtension(file, ".pdb")));
        }
        foreach (var file in Directory.GetFiles(Path.Combine(data, "Plugins"), "Firebase*.dll", SearchOption.AllDirectories))
            removedFiles.Add(Path.GetRelativePath(data, file));
        foreach (var name in new[] { "google-services-desktop.json", "UnityServicesProjectConfiguration.json" })
        {
            var relative = Path.Combine("StreamingAssets", name);
            if (File.Exists(Path.Combine(data, relative))) removedFiles.Add(relative);
        }
        foreach (var name in new[] { "RuntimeInitializeOnLoads.json", "ScriptingAssemblies.json" })
        {
            var json = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(Path.Combine(data, name)))!;
            if (name == "RuntimeInitializeOnLoads.json")
            {
                var hooks = json["root"]!.AsArray();
                foreach (var hook in hooks.ToArray())
                {
                    var assembly = hook!["assemblyName"]!.GetValue<string>();
                    var ns = hook["nameSpace"]!.GetValue<string>();
                    var type = (ns.Length == 0 ? "" : ns + ".") + hook["className"]!.GetValue<string>();
                    if (IsTelemetryAssembly(assembly) || removedTypes.Contains(assembly + ":" + type)) hooks.Remove(hook);
                }
            }
            else
            {
                var names = json["names"]!.AsArray();
                var types = json["types"]!.AsArray();
                for (int i = names.Count - 1; i >= 0; i--)
                    if (IsTelemetryAssembly(Path.GetFileNameWithoutExtension(names[i]!.GetValue<string>())))
                    { names.RemoveAt(i); types.RemoveAt(i); }
            }
            File.WriteAllText(Path.Combine(stage, name), json.ToJsonString());
            modifiedFiles.Add(name);
        }
        var manifest = new { removedFiles, modifiedFiles, removedTypes };
        File.WriteAllText(Path.Combine(stage, "manifest.json"), JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));
        if (!stageOnly)
            throw new InvalidOperationException("Use --stage-only, then scripts/remove_client_telemetry.py to install with serialized assets and backups.");
        Console.WriteLine($"Staged {modifiedFiles.Count} modified files and {removedFiles.Count} removals at {stage}");
    }

    static void VerifyNoTelemetry(string clientRoot, string? overlay)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        var resolver = new DefaultAssemblyResolver();
        if (overlay != null) resolver.AddSearchDirectory(Path.Combine(overlay, "Managed"));
        resolver.AddSearchDirectory(managed);
        int count = 0;
        foreach (var original in Directory.GetFiles(managed, "*.dll"))
        {
            if (IsTelemetryAssembly(Path.GetFileNameWithoutExtension(original)))
            {
                if (overlay == null) throw new InvalidOperationException($"Telemetry SDK still installed: {original}");
                continue;
            }
            var staged = overlay == null ? original : Path.Combine(overlay, "Managed", Path.GetFileName(original));
            var path = File.Exists(staged) ? staged : original;
            using var assembly = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { AssemblyResolver = resolver });
            var module = assembly.MainModule;
            var badTypes = module.GetTypes().Where(IsTelemetryType).Select(t => t.FullName)
                .Concat(module.GetTypeReferences().Where(IsTelemetryType).Select(t => t.FullName)).ToArray();
            var badAssemblies = module.AssemblyReferences.Where(r => IsTelemetryAssembly(r.Name) || r.Name == "System.Private.CoreLib").ToArray();
            if (badTypes.Length != 0 || badAssemblies.Length != 0)
                throw new InvalidOperationException($"Telemetry or incompatible references in {module.Name}: " +
                    string.Join(", ", badTypes.Concat(badAssemblies.Select(r => r.Name))));
            foreach (var method in module.GetTypes().SelectMany(t => t.Methods).Where(m => m.HasBody))
            {
                foreach (var instruction in method.Body.Instructions)
                {
                    if (instruction.Operand is MethodReference call && IsTelemetryMethod(call))
                        throw new InvalidOperationException($"Telemetry call remains: {method.FullName}: {instruction}");
                    if (instruction.OpCode == OpCodes.Ldstr && instruction.Operand is string text &&
                        (text.Contains("/api/analytics/", StringComparison.Ordinal) || text == "https://qa.masanggames.com"))
                        throw new InvalidOperationException($"Telemetry endpoint remains in {method.FullName}");
                }
            }
            count++;
        }
        Console.WriteLine($"Verified {count} managed assemblies: no removed telemetry types, references, calls, or Masang reporting endpoints.");
    }
}
