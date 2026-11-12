using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program
{
    static void PatchMissionThumbnails(ModuleDefinition module)
    {
        const string prefix = "UI/Quest/SubCategory/";
        var type = module.GetType("NGame2.NUI.NComponent2.MissionSubCategoryComponent");
        var method = type.Methods.Single(m => m.Name == "SetupSubCategoryImage");
        if (method.Body.Instructions.Any(i => Equals(i.Operand, prefix))) return;

        // Restored Mission tables use sprite names from the old Quest2 atlas.
        // The current BundlePack registers these sprites in Quest.SubCategory
        // using full paths. Keep empty and already-qualified references intact.
        var getter = method.Body.Instructions.Single(i =>
            i.Operand is MethodReference r && r.Name == "get_SubCategoryButtonPath");
        var assign = getter.Next;
        if (assign?.Operand is not MethodReference setter || setter.Name != "set_AssetPath")
            throw new InvalidOperationException("Unexpected Mission thumbnail assignment IL");

        MethodReference StringMethod(string name, TypeReference result, bool instance, params TypeReference[] parameters)
        {
            var reference = new MethodReference(name, result, module.TypeSystem.String) { HasThis = instance };
            foreach (var parameter in parameters) reference.Parameters.Add(new ParameterDefinition(parameter));
            return reference;
        }
        var empty = StringMethod("IsNullOrEmpty", module.TypeSystem.Boolean, false, module.TypeSystem.String);
        var indexOf = StringMethod("IndexOf", module.TypeSystem.Int32, true, module.TypeSystem.String);
        var concat = StringMethod("Concat", module.TypeSystem.String, false, module.TypeSystem.String, module.TypeSystem.String);
        var path = new VariableDefinition(module.TypeSystem.String);
        method.Body.Variables.Add(path);
        method.Body.InitLocals = true;
        var loadPath = Instruction.Create(OpCodes.Ldloc, path);
        var instructions = new[] {
            Instruction.Create(OpCodes.Stloc, path),
            Instruction.Create(OpCodes.Ldloc, path),
            Instruction.Create(OpCodes.Call, empty),
            Instruction.Create(OpCodes.Brtrue, loadPath),
            Instruction.Create(OpCodes.Ldloc, path),
            Instruction.Create(OpCodes.Ldstr, "/"),
            Instruction.Create(OpCodes.Callvirt, indexOf),
            Instruction.Create(OpCodes.Ldc_I4_0),
            Instruction.Create(OpCodes.Bge, loadPath),
            Instruction.Create(OpCodes.Ldstr, prefix),
            Instruction.Create(OpCodes.Ldloc, path),
            Instruction.Create(OpCodes.Call, concat),
            Instruction.Create(OpCodes.Stloc, path),
            loadPath
        };
        var il = method.Body.GetILProcessor();
        foreach (var instruction in instructions) il.InsertBefore(assign, instruction);
        Console.WriteLine("Qualified Mission subcategory thumbnail references for the current atlas.");
    }

    static void InstallMissionThumbnails(string clientRoot, bool stageOnly)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        var path = Path.Combine(managed, "Assembly-CSharp.dll");
        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using var assembly = AssemblyDefinition.ReadAssembly(path,
            new ReaderParameters { AssemblyResolver = resolver, InMemory = true });
        PatchMissionThumbnails(assembly.MainModule);
        var staged = path + ".mission-thumbnails-staged";
        assembly.Write(staged);
        if (stageOnly) { Console.WriteLine("Staged " + staged); return; }
        var backup = Path.Combine("target", "mission-thumbnail-backups", DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        Directory.CreateDirectory(backup);
        File.Copy(path, Path.Combine(backup, "Assembly-CSharp.dll"));
        File.Copy(staged, path, true);
        Console.WriteLine("Installed focused Mission thumbnail patch. Backup: " + backup);
    }
}
