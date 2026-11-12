using System.Security.Cryptography;
using System.Text.Json;
using Mono.Cecil;

// This process only reads metadata; it never loads or executes game assemblies.
if (args.Length < 3) throw new ArgumentException("inspect <assembly.dll> <type> [method] | inventory <managed-dir> <output.json> | verify-build <reference-managed-dir> <built-managed-dir>");
var json = new JsonSerializerOptions { WriteIndented = true };
if (args[0] == "inventory")
{
    using var resolver = new DefaultAssemblyResolver();
    resolver.AddSearchDirectory(args[1]);
    var assemblies = Directory.GetFiles(args[1], "*.dll").OrderBy(p => p, StringComparer.Ordinal).Select(path =>
    {
        using var a = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { AssemblyResolver = resolver });
        var m = a.MainModule;
        var types = m.GetTypes().ToArray();
        return new {
            file = Path.GetFileName(path), sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant(),
            identity = a.Name.FullName, runtime = m.RuntimeVersion, architecture = m.Architecture.ToString(),
            attributes = m.Attributes.ToString(), references = m.AssemblyReferences.Select(r => r.FullName).ToArray(),
            assemblyAttributes = a.CustomAttributes.Select(Attr).ToArray(),
            interop = types.SelectMany(t => t.Methods).Where(x => x.IsPInvokeImpl).Select(x => new {
                method = x.FullName, module = x.PInvokeInfo.Module.Name, entryPoint = x.PInvokeInfo.EntryPoint,
                attributes = x.PInvokeInfo.Attributes.ToString()
            }).ToArray(),
            dynamicCalls = types.SelectMany(t => t.Methods).Where(x => x.HasBody).SelectMany(x => x.Body.Instructions
                .Where(i => i.Operand is MethodReference r && (r.DeclaringType.FullName.StartsWith("System.Reflection.") ||
                    r.DeclaringType.FullName == "System.Type" || r.Name == "SendMessage"))
                .Select(i => new { caller = x.FullName, target = ((MethodReference)i.Operand).FullName })).Distinct().ToArray(),
            // All fields, including private ones: Unity and reflection depend on them.
            types = types.Select(t => new {
                name = t.FullName, attributes = t.Attributes.ToString(), baseType = t.BaseType?.FullName,
                interfaces = t.Interfaces.Select(i => i.InterfaceType.FullName).ToArray(),
                customAttributes = t.CustomAttributes.Select(Attr).ToArray(),
                fields = t.Fields.Select(f => new { signature = f.FullName, attributes = f.Attributes.ToString(),
                    customAttributes = f.CustomAttributes.Select(Attr).ToArray() }).ToArray(),
                methods = t.Methods.Select(f => new { signature = f.FullName, attributes = f.Attributes.ToString(),
                    customAttributes = f.CustomAttributes.Select(Attr).ToArray() }).ToArray()
            }).ToArray()
        };
    }).ToArray();
    File.WriteAllText(args[2], JsonSerializer.Serialize(assemblies, json));
}
else if (args[0] == "verify-build")
{
    var count = 0;
    foreach (var file in Directory.GetFiles(args[2], "*.dll").OrderBy(p => p, StringComparer.Ordinal))
    {
        using var original = AssemblyDefinition.ReadAssembly(Path.Combine(args[1], Path.GetFileName(file)));
        using var rebuilt = AssemblyDefinition.ReadAssembly(file);
        if (original.Name.FullName != rebuilt.Name.FullName)
            throw new InvalidDataException($"Assembly identity changed: {Path.GetFileName(file)}");
        var before = original.MainModule.Resources.OfType<EmbeddedResource>().ToDictionary(r => r.Name, r => Convert.ToHexString(SHA256.HashData(r.GetResourceData())));
        var after = rebuilt.MainModule.Resources.OfType<EmbeddedResource>().ToDictionary(r => r.Name, r => Convert.ToHexString(SHA256.HashData(r.GetResourceData())));
        if (before.Count != after.Count || before.Any(p => !after.TryGetValue(p.Key, out var hash) || hash != p.Value))
            throw new InvalidDataException($"Embedded resource changed: {Path.GetFileName(file)}");
        foreach (var reference in rebuilt.MainModule.AssemblyReferences)
        {
            var resolved = Path.Combine(args[2], reference.Name + ".dll");
            if (!File.Exists(resolved)) resolved = Path.Combine(args[1], reference.Name + ".dll");
            if (!File.Exists(resolved)) throw new InvalidDataException($"Missing dependency: {reference.FullName}");
            using var dependency = AssemblyDefinition.ReadAssembly(resolved);
            if (dependency.Name.FullName != reference.FullName)
                throw new InvalidDataException($"Dependency identity mismatch: {reference.FullName}");
        }
        count++;
    }
    if (count == 0) throw new InvalidDataException("No compiled assemblies to verify");
    Console.WriteLine($"Verified {count} assembly identities, dependency identities and embedded resources.");
}
else if (args[0] == "inspect")
{
    using var assembly = AssemblyDefinition.ReadAssembly(args[1]);
    foreach (var type in assembly.MainModule.GetTypes().Where(t => t.FullName == args[2]))
    foreach (var method in type.Methods.Where(m => args.Length < 4 || m.Name.Contains(args[3], StringComparison.Ordinal)))
    {
        Console.WriteLine(method.FullName);
        foreach (var ov in method.Overrides) Console.WriteLine("  override " + ov.FullName);
        if (method.HasBody) foreach (var instruction in method.Body.Instructions) Console.WriteLine("  " + instruction);
    }
}
else throw new ArgumentException("Unknown command");

static string Attr(CustomAttribute a) => a.AttributeType.FullName + "(" +
    string.Join(",", a.ConstructorArguments.Select(x => x.Value?.ToString())) + ") blob=" + Convert.ToHexString(a.GetBlob());
