using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program {
    static void VerifyRoomShortcut(ModuleDefinition module) {
        var names=new[]{"OnClickShortcut","OnClickMoveDirect","OnClickRestoreSoulStone"};
        for(int mode=0;mode<names.Length;mode++) {
            var name=names[mode];
            var method=module.GetType("NGame2.NUI.NWindow.InvenItemDesc").Methods.Single(m=>m.Name==name);
            var code=method.Body.Instructions;
            if(code.Count<6 || code[0].OpCode!=OpCodes.Ldarg_0 || code[1].OpCode!=OpCodes.Ldc_I4 || !Equals(code[1].Operand,mode)
                || code[2].Operand is not MethodReference call || call.DeclaringType.Name!="SprkRoomShortcut" || call.Name!="Intercept"
                || code[3].OpCode!=OpCodes.Brfalse || code[3].Operand!=code[5] || code[4].OpCode!=OpCodes.Ret
                || code.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkRoomShortcut")!=1)
                throw new InvalidOperationException("Room shortcut entry guard missing: "+name);
        }
    }

    static void VerifyRoomShortcutFile(string root,bool staged=false) {
        var managed=Path.Combine(root,"King's Raid_Data","Managed");
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"+(staged?".room-shortcut-staged":"")));
        VerifyRoomShortcut(game.MainModule);
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkRoomShortcut.dll"+(staged?".staged":"")));
        var calls=helper.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand).OfType<MethodReference>().Select(m=>m.Name).ToArray();
        foreach(var required in new[]{"get_PartyRoom","Close","CloseAll","ForEach","StartCoroutine","OnClickShortcut","OnClickMoveDirect","OnClickRestoreSoulStone"})
            if(!calls.Contains(required))throw new InvalidOperationException("Incomplete room shortcut transition: "+required);
        Console.WriteLine("Verified shortcut guards and native room-close completion routing.");
    }

    static void InstallRoomShortcut(string root,bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");
        var helperPath=Path.Combine(managed,"SprkRoomShortcut.dll");
        using var resource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.RoomShortcut.cs.txt")!;
        using var reader=new StreamReader(resource);
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>p!=helperPath).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("SprkRoomShortcut",new[]{CSharpSyntaxTree.ParseText(reader.ReadToEnd())},refs,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
        using var compiled=new MemoryStream();
        var result=compilation.Emit(compiled);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        compiled.Position=0;
        using var helper=AssemblyDefinition.ReadAssembly(compiled);
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        var names=new[]{"OnClickShortcut","OnClickMoveDirect","OnClickRestoreSoulStone"};
        for(int mode=0;mode<names.Length;mode++) {
            var method=game.MainModule.GetType("NGame2.NUI.NWindow.InvenItemDesc").Methods.Single(m=>m.Name==names[mode]);
            if(method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkRoomShortcut"))continue;
            var first=method.Body.Instructions[0];var il=method.Body.GetILProcessor();
            il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first,il.Create(OpCodes.Ldc_I4,mode));
            il.InsertBefore(first,il.Create(OpCodes.Call,game.MainModule.ImportReference(helper.MainModule.GetType("SprkRoomShortcut").Methods.Single(m=>m.Name=="Intercept"))));
            il.InsertBefore(first,il.Create(OpCodes.Brfalse,first));
            il.InsertBefore(first,il.Create(OpCodes.Ret));
        }

        VerifyRoomShortcut(game.MainModule);
        var staged=dll+".room-shortcut-staged";
        game.Write(staged);File.WriteAllBytes(helperPath+".staged",compiled.ToArray());
        VerifyRoomShortcutFile(root,true);
        if(stageOnly)return;
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))
            throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
        File.Copy(dll,dll+".before-room-shortcut-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
        File.Move(staged,dll,true);File.Move(helperPath+".staged",helperPath,true);
        VerifyRoomShortcutFile(root);
    }
}
