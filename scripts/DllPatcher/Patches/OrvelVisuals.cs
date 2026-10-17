using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    const string OrvelVisualsType = "SprkOrvelVisuals";
    static readonly string[] OrvelHiddenProps = { "UI.EventPopup", "UI.EventPopup3", "UI.EventPopupFire", "UI.Node.Portal" };

    static void PatchOrvelVisuals(ModuleDefinition module)
    {
        var prop = module.GetType("NShared.WorldMapPropData");
        var helper = module.GetType(OrvelVisualsType);
        if (helper == null)
        {
            helper = new TypeDefinition("", OrvelVisualsType,
                TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit, module.TypeSystem.Object);
            module.Types.Add(helper);
        }
        var filter = helper.Methods.SingleOrDefault(m => m.Name == "FilterProp");
        if (filter == null)
        {
            filter = new MethodDefinition("FilterProp", MethodAttributes.Public | MethodAttributes.Static, prop);
            filter.Parameters.Add(new ParameterDefinition("prop", ParameterAttributes.None, prop));
            helper.Methods.Add(filter);
        }
        filter.Body = new MethodBody(filter) { MaxStackSize = 2 };
        var il = filter.Body.GetILProcessor();
        var keep = Instruction.Create(OpCodes.Ldarg_0);
        var hide = Instruction.Create(OpCodes.Ldnull);
        var equal = new MethodReference("op_Equality", module.TypeSystem.Boolean, module.TypeSystem.String) { HasThis = false };
        equal.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));
        equal.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));
        il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Brfalse, keep);
        il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Callvirt, prop.Methods.Single(m => m.Name == "get_ChapterIndex"));
        il.Emit(OpCodes.Ldc_I4, 99); il.Emit(OpCodes.Bne_Un, keep);
        foreach (var name in OrvelHiddenProps)
        {
            il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Callvirt, prop.Methods.Single(m => m.Name == "get_PropName"));
            il.Emit(OpCodes.Ldstr, name); il.Emit(OpCodes.Call, equal); il.Emit(OpCodes.Brtrue, hide);
        }
        il.Append(keep); il.Emit(OpCodes.Ret); il.Append(hide); il.Emit(OpCodes.Ret);

        var refresh = module.GetType("NGame2.NUI.NWorldMap.NMapLayout.PropManager").Methods.Single(m => m.Name == "RefreshProp");
        if (!refresh.Body.Instructions.Any(i => i.Operand is MethodReference m && m.DeclaringType.Name == OrvelVisualsType))
        {
            var read = refresh.Body.Instructions.Single(i => i.Operand is MethodReference m && m.Name == "get_Item"
                && m.DeclaringType is GenericInstanceType g && g.GenericArguments.Count == 1 && g.GenericArguments[0].FullName == prop.FullName);
            // The native loop already skips null props before condition checks
            // and asset creation. Keep the table and its cached list untouched.
            refresh.Body.GetILProcessor().InsertAfter(read, Instruction.Create(OpCodes.Call, filter));
        }
        Console.WriteLine("Filtered Orvel EVENT effects and the red event portal before prop creation.");
    }

    static void InstallOrvelVisuals(string clientRoot, bool stageOnly)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var staged = dll + ".orvel-visuals-staged";
        using var resolver = new DefaultAssemblyResolver(); resolver.AddSearchDirectory(managed);
        using var game = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver });
        PatchOrvelVisuals(game.MainModule); game.Write(staged);
        VerifyOrvelVisualsFile(staged);
        if (!stageOnly)
        {
            var backup = Path.Combine(Environment.CurrentDirectory, "target", "orvel-visuals-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup); File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
            game.Dispose(); File.Move(staged, dll, true);
        }
        Console.WriteLine(stageOnly ? "Staged Orvel visual cleanup." : "Installed Orvel visual cleanup. Restart the client to load it.");
    }

    static void VerifyOrvelVisualsFile(string path)
    {
        using var game = AssemblyDefinition.ReadAssembly(path);
        var module = game.MainModule;
        var filter = module.GetType(OrvelVisualsType).Methods.Single(m => m.Name == "FilterProp");
        var names = filter.Body.Instructions.Where(i => i.OpCode == OpCodes.Ldstr).Select(i => (string)i.Operand).ToArray();
        if (!names.SequenceEqual(OrvelHiddenProps) || !filter.Body.Instructions.Any(i => i.OpCode == OpCodes.Ldc_I4 && Equals(i.Operand, 99)))
            throw new InvalidOperationException("Orvel visual filter has an unexpected scope.");
        var refresh = module.GetType("NGame2.NUI.NWorldMap.NMapLayout.PropManager").Methods.Single(m => m.Name == "RefreshProp");
        var hooks = refresh.Body.Instructions.Where(i => i.Operand is MethodReference m && m.DeclaringType.Name == OrvelVisualsType).ToArray();
        if (hooks.Length != 1 || hooks[0].Previous.Operand is not MethodReference read || read.Name != "get_Item")
            throw new InvalidOperationException("Orvel visual filter must run once immediately after each native prop read.");
        Console.WriteLine("Verified Orvel-only prop filter and native creation hook.");
    }
}
