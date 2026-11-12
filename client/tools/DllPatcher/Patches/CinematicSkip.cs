using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    const string VideoManagerType = "NGame2.NUI.NWindow.VideoManager";

    static void PatchCinematicSkip(ModuleDefinition module)
    {
        var type = module.GetType(VideoManagerType);
        var skip = type.Methods.Single(m => m.Name == "SkipToClose" && m.IsStatic && m.Parameters.Count == 1);
        var accept = type.Methods.Single(m => m.Name == "OnSkipButtonPressed" && m.IsStatic && m.Parameters.Count == 1);
        if (!skip.Body.Instructions.Any(i => i.Operand is MethodReference r &&
                r.DeclaringType.Name == "MessagePopup3Button" && r.Name == "OpenGlobal")) {
            VerifyCinematicSkip(module);
            return;
        }
        var invoke = new MethodReference("Invoke", module.TypeSystem.Void, accept.ReturnType) { HasThis = true };
        // Run the original Yes callback. VideoManager's update/stop lifecycle
        // still stops playback, restores camera/audio and invokes completion.
        skip.Body = new MethodBody(skip);
        var il = skip.Body.GetILProcessor();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, accept);
        il.Emit(OpCodes.Callvirt, invoke);
        il.Emit(OpCodes.Ret);
        VerifyCinematicSkip(module);
    }

    static void VerifyCinematicSkip(ModuleDefinition module)
    {
        var type = module.GetType(VideoManagerType);
        var skip = type.Methods.Single(m => m.Name == "SkipToClose");
        var instructions = skip.Body.Instructions;
        if (instructions.Count != 4 || instructions[0].OpCode != OpCodes.Ldarg_0 ||
            instructions[1].OpCode != OpCodes.Call || instructions[1].Operand is not MethodReference accept ||
            accept.DeclaringType.FullName != VideoManagerType || accept.Name != "OnSkipButtonPressed" ||
            instructions[2].OpCode != OpCodes.Callvirt || instructions[2].Operand is not MethodReference invoke ||
            invoke.DeclaringType.FullName != "System.Action" || invoke.Name != "Invoke" ||
            instructions[3].OpCode != OpCodes.Ret)
            throw new InvalidOperationException("Cinematic Skip must invoke the original confirmation action directly.");
        var factory = type.Methods.Single(m => m.Name == "OnSkipButtonPressed");
        var callbackReference = factory.Body.Instructions.Select(i => i.Operand).OfType<MethodReference>()
            .Single(m => m.Name.Contains("<OnSkipButtonPressed>"));
        var callback = callbackReference.Resolve();
        foreach (var field in new[] { "finishCallback", "isSkipped" })
            if (!callback.Body.Instructions.Any(i => i.OpCode == OpCodes.Stfld &&
                    i.Operand is FieldReference f && f.DeclaringType.FullName == VideoManagerType && f.Name == field))
                throw new InvalidOperationException("Missing original cinematic skip effect: " + field);
        var stop = type.Methods.Single(m => m.HasBody && m.Body.Instructions.Any(i =>
            i.Operand is MethodReference call && call.Name == "fire") &&
            m.Body.Instructions.Any(i => i.Operand is MethodReference call &&
                call.DeclaringType.FullName == "UnityEngine.Video.VideoPlayer" && call.Name == "Stop"));
        foreach (var name in new[] { "Stop", "EnableVideoCamera", "fire" })
            if (!stop.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == name))
                throw new InvalidOperationException("Missing native video finalization: " + name);
        Console.WriteLine("Verified immediate cinematic Skip and retained native completion/cleanup callbacks.");
    }

    static void VerifyCinematicSkipFile(string clientRoot)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(managed, "Assembly-CSharp.dll"),
            new ReaderParameters { AssemblyResolver = resolver });
        VerifyCinematicSkip(assembly.MainModule);
    }

    static void InstallCinematicSkip(string clientRoot, bool stageOnly)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        var path = Path.Combine(managed, "Assembly-CSharp.dll");
        var staged = path + ".cinematic-skip-staged";
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using (var assembly = AssemblyDefinition.ReadAssembly(path,
                   new ReaderParameters { AssemblyResolver = resolver, InMemory = true })) {
            PatchCinematicSkip(assembly.MainModule);
            assembly.Write(staged);
        }
        using (var assembly = AssemblyDefinition.ReadAssembly(staged,
                   new ReaderParameters { AssemblyResolver = resolver })) VerifyCinematicSkip(assembly.MainModule);
        if (!stageOnly) {
            var backup = Path.Combine("target", "cinematic-skip-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup);
            File.Copy(path, Path.Combine(backup, "Assembly-CSharp.dll"));
            File.Move(staged, path, true);
        }
        Console.WriteLine(stageOnly ? "Staged cinematic Skip without confirmation." : "Installed immediate cinematic Skip. Restart the client.");
    }
}
