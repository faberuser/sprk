using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static void InstallAutomation(string clientRoot)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var helper = Path.Combine(managed, "SprkAutomation.dll");
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.Automation.cs.txt")!;
        using var reader = new StreamReader(stream);
        var references = Directory.GetFiles(managed, "*.dll")
            .Where(p => Path.GetFileName(p) != "SprkAutomation.dll")
            .Select(p => MetadataReference.CreateFromFile(p));
        var compilation = CSharpCompilation.Create("SprkAutomation",
            new[] { CSharpSyntaxTree.ParseText(reader.ReadToEnd()) }, references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));
        using var compiled = new MemoryStream();
        var emitted = compilation.Emit(compiled);
        if (!emitted.Success) throw new InvalidOperationException(string.Join("\n", emitted.Diagnostics));
        compiled.Position = 0;
        using var helperAssembly = AssemblyDefinition.ReadAssembly(compiled);
        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver });
        var awake = assembly.MainModule.Types.Single(t => t.Name == "UICamera").Methods.Single(m => m.Name == "Awake");
        var installed = awake.Body.Instructions.Any(i => i.Operand is MethodReference m && m.DeclaringType.FullName == "SprkAutomation" && m.Name == "Ensure");
        bool changed = !installed;
        if (!installed)
        {
            var ensure = helperAssembly.MainModule.Types.Single(t => t.Name == "SprkAutomation").Methods.Single(m => m.Name == "Ensure");
            awake.Body.GetILProcessor().InsertBefore(awake.Body.Instructions[0], Instruction.Create(OpCodes.Call, assembly.MainModule.ImportReference(ensure)));
        }
        var prefs = helperAssembly.MainModule.Types.Single(t => t.Name == "SprkTestPrefs");
        foreach(var type in assembly.MainModule.GetTypes())
        foreach(var method in type.Methods.Where(m => m.HasBody))
        foreach(var instruction in method.Body.Instructions)
        {
            if(instruction.Operand is MethodReference device && device.DeclaringType.FullName == "UnityEngine.SystemInfo" && device.Name == "get_deviceUniqueIdentifier") {
                instruction.Operand = assembly.MainModule.ImportReference(prefs.Methods.Single(m => m.Name == "DeviceId"));
                changed = true;
                continue;
            }
            if(instruction.Operand is not MethodReference call || call.DeclaringType.FullName != "UnityEngine.PlayerPrefs") continue;
            var replacement = prefs.Methods.SingleOrDefault(m => m.Name == call.Name &&
                m.Parameters.Select(p => p.ParameterType.FullName).SequenceEqual(call.Parameters.Select(p => p.ParameterType.FullName)));
            if(replacement == null) throw new InvalidOperationException("Unsupported PlayerPrefs call: " + call.FullName);
            instruction.Operand = assembly.MainModule.ImportReference(replacement);
            changed = true;
        }
        var stamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
        // Preserve the currently installed restoration patches, not the old original DLL.
        if (changed) File.Copy(dll, dll + ".before_automation_" + stamp);
        if (File.Exists(helper)) File.Copy(helper, helper + ".backup_" + stamp);
        File.WriteAllBytes(helper + ".staged", compiled.ToArray());
        if (changed) assembly.Write(dll + ".staged");
        assembly.Dispose();
        File.Move(helper + ".staged", helper, true);
        if (changed) File.Move(dll + ".staged", dll, true);
        Console.WriteLine("Installed opt-in client automation. Set SPRK_AUTOMATION_DIR only in the test client process.");
    }
}
