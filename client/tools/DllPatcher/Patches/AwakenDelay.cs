using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static MethodDefinition AwakeningAnimation(ModuleDefinition module) => module
        .GetType("NGame2.NUI.NWindow.PurifyHeroRoom").NestedTypes.Single(t => t.Name.StartsWith("<coAwakeHeroAnim>"))
        .Methods.Single(m => m.Name == "MoveNext");

    static void PatchAwakenDelay(ModuleDefinition module)
    {
        PatchPurifyHudReuse(module);
        var method = AwakeningAnimation(module);
        var waits = method.Body.Instructions.Where(i => i.OpCode == OpCodes.Newobj &&
            i.Operand is MethodReference r && r.DeclaringType.FullName == "UnityEngine.WaitForSeconds").ToArray();
        if (waits.Length == 0) { VerifyAwakenDelay(module); return; }
        if (waits.Length != 1 || waits[0].Previous.OpCode != OpCodes.Ldc_R4 || !Equals(waits[0].Previous.Operand, 3f))
            throw new InvalidOperationException("Unexpected awakening delay; refusing to patch.");
        // Yield for one frame instead of waiting three seconds. Keep the native
        // request success checks, full-screen cinematic loading, and callbacks.
        waits[0].Previous.OpCode = OpCodes.Ldnull;
        waits[0].Previous.Operand = null;
        waits[0].OpCode = OpCodes.Nop;
        waits[0].Operand = null;
        VerifyAwakenDelay(module);
    }

    static void VerifyAwakenDelay(ModuleDefinition module)
    {
        VerifyPurifyHudReuse(module);
        var body = AwakeningAnimation(module).Body.Instructions;
        if (body.Any(i => i.Operand is MethodReference r && r.DeclaringType.FullName == "UnityEngine.WaitForSeconds") ||
            body.Count(i => i.OpCode == OpCodes.Stfld && i.Operand is FieldReference f && f.Name == "<>2__current" &&
                i.Previous.OpCode == OpCodes.Nop && i.Previous.Previous.OpCode == OpCodes.Ldnull) != 1)
            throw new InvalidOperationException("Awakening must yield one frame without a timed delay.");
        foreach (var name in new[] { "PlayAwake", "OpenGetNewHero", "Close", "Apply", "fire" })
            if (!body.Any(i => i.Operand is MethodReference r && r.Name == name))
                throw new InvalidOperationException("Missing native awakening operation: " + name);
    }

    static void PatchPurifyHudReuse(ModuleDefinition module)
    {
        var room=module.GetType("NGame2.NUI.NWindow.PurifyHeroRoom");
        var tween=module.GetType("UITweener");
        var reset=tween.Methods.Single(m=>m.Name=="ResetToBeginning" && m.Parameters.Count==0);
        var helper=room.Methods.SingleOrDefault(m=>m.Name=="SprkResetPurifyTween");
        if(helper==null) {
            helper=new MethodDefinition("SprkResetPurifyTween",MethodAttributes.Private|MethodAttributes.Static|MethodAttributes.HideBySig,module.TypeSystem.Void);
            helper.Parameters.Add(new ParameterDefinition("tween",ParameterAttributes.None,tween));room.Methods.Add(helper);
            var enabled=tween.Methods.Single(m=>m.Name=="Play" && m.Parameters.Count==1 && m.Parameters[0].ParameterType.MetadataType==MetadataType.Boolean)
                .Body.Instructions.Select(i=>i.Operand).OfType<MethodReference>().Single(m=>m.Name=="set_enabled");
            var il=helper.Body.GetILProcessor();
            // ResetToBeginning samples the visible start pose but leaves the
            // previous exit tween enabled. Stop it before reusing this HUD.
            il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldc_I4_0);il.Emit(OpCodes.Callvirt,enabled);
            il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Callvirt,reset);il.Emit(OpCodes.Ret);
        }
        var init=room.Methods.Single(m=>m.Name=="Init");
        foreach(var instruction in init.Body.Instructions.Where(i=>i.Operand is MethodReference m && m.FullName==reset.FullName)) {
            instruction.OpCode=OpCodes.Call;instruction.Operand=helper;
        }
        VerifyPurifyHudReuse(module);
    }

    static void VerifyPurifyHudReuse(ModuleDefinition module)
    {
        var room=module.GetType("NGame2.NUI.NWindow.PurifyHeroRoom");
        var helper=room.Methods.Single(m=>m.Name=="SprkResetPurifyTween");
        var body=helper.Body.Instructions;
        if(body.Count!=6 || body[1].OpCode!=OpCodes.Ldc_I4_0 || body[2].Operand is not MethodReference enabled || enabled.Name!="set_enabled" ||
            body[4].Operand is not MethodReference reset || reset.Name!="ResetToBeginning")
            throw new InvalidOperationException("Purification HUD must stop its exit tween before resetting its visible pose.");
        var init=room.Methods.Single(m=>m.Name=="Init");
        var hooks=init.Body.Instructions.Where(i=>i.Operand is MethodReference m && m.FullName==helper.FullName).ToArray();
        if(hooks.Length!=3 || !hooks.Select(i=>(i.Previous.Operand as FieldReference)?.Name).ToHashSet().SetEquals(new[]{"BottomTween","TopTween","HighlightTween"}) ||
            init.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="ResetToBeginning"))
            throw new InvalidOperationException("Purification must reset all three HUD tweens on every hero initialization.");
        VerifyArenaResultStack(helper);VerifyArenaResultStack(init);
    }

    static void InstallAwakenDelay(string clientRoot, bool stageOnly)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var staged = dll + ".awaken-delay-staged";
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver })) {
            PatchAwakenDelay(assembly.MainModule);
            assembly.Write(staged);
        }
        using (var verified = AssemblyDefinition.ReadAssembly(staged)) VerifyAwakenDelay(verified.MainModule);
        if (!stageOnly) {
            var backup = Path.Combine("target", "awaken-delay-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup);
            File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
            File.Move(staged, dll, true);
        }
        Console.WriteLine(stageOnly ? "Staged and verified removal of the three-second awakening delay." :
            "Installed and verified awakening delay removal. Restart the client.");
    }
}
