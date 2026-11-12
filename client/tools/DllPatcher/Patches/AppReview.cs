using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static void PatchAppReviewPopup(ModuleDefinition module)
    {
        var popup = module.GetType("NGame2.NUI.NWindow.AppReviewPopup")
            ?? throw new InvalidOperationException("App review popup type was not found.");
        foreach (var name in new[] { "Open", "ForceOpen" })
        {
            var method = popup.Methods.Single(m => m.Name == name && m.IsStatic && !m.HasParameters);
            var expected = name == "Open" ? MetadataType.Boolean : MetadataType.Void;
            if (!method.HasBody || method.ReturnType.MetadataType != expected)
                throw new InvalidOperationException("Unrecognized app review opening method: " + method.FullName);
            method.Body = new MethodBody(method);
            var il = method.Body.GetILProcessor();
            if (expected == MetadataType.Boolean) il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ret);
        }
        Console.WriteLine("Disabled automatic and forced app-store review popup opening.");
    }

    static void InstallAppReviewRemoval(string clientRoot, bool stageOnly)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var staged = dll + ".app-review-staged";
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver });
        PatchAppReviewPopup(assembly.MainModule);
        assembly.Write(staged);
        if (!stageOnly)
        {
            var backup = Path.Combine(Environment.CurrentDirectory, "target", "app-review-backups",
                DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup);
            File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
            assembly.Dispose();
            File.Move(staged, dll, true);
        }
        Console.WriteLine(stageOnly ? $"Staged app review removal: {staged}" : "Installed app review removal. Restart the client to load it.");
    }
}
