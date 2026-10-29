using Mono.Cecil;
using Mono.Cecil.Cil;
namespace DllPatcher;
partial class Program {
    static void VerifyClientStorageFiles(string settings,string helperPath,string assemblyPath) {
        var bytes=File.ReadAllBytes(settings);var expected=IsolatedPlayerSettings(OriginalPlayerSettings());
        bool company=false;
        foreach(var value in new[]{0,1})foreach(var logging in new[]{0,1}){expected[640]=(byte)value;expected[402]=(byte)logging;int start=bytes.AsSpan().IndexOf(expected);if(start>=0){if(company || bytes.AsSpan(start+1).IndexOf(expected)>=0)throw new InvalidOperationException("Ambiguous isolated company payload.");company=true;}}
        if(!company)throw new InvalidOperationException("Client company is not sprk.");
        FindFrameTimingSettings(bytes,out _);
        using var helper=AssemblyDefinition.ReadAssembly(helperPath);using var game=AssemblyDefinition.ReadAssembly(assemblyPath);
        var module=helper.MainModule;
        if(module.GetType("SprkStorageMigration")==null || module.GetType("SprkStorageReporter")==null)throw new InvalidOperationException("Missing storage migration/reporting code.");
        foreach(var (type,method,hook) in new[]{("SprkTestPrefs","Prefix","Ensure"),("SprkAutomation","Ensure","StartReporter")}) {
            var target=module.GetType(type).Methods.Single(m=>m.Name==method);
            if(!target.Body.Instructions.Any(i=>i.Operand is MethodReference m&&m.DeclaringType.Name=="SprkStorageMigration"&&m.Name==hook))throw new InvalidOperationException("Missing migration startup hook: "+type);
        }
        var camera=game.MainModule.GetType("UICamera").Methods.Single(m=>m.Name=="Awake");
        if(!camera.Body.Instructions.Any(i=>i.Operand is MethodReference m&&m.DeclaringType.Name=="SprkAutomation"&&m.Name=="Ensure"))throw new InvalidOperationException("Missing native automation startup hook.");
        Console.WriteLine("Verified sprk company/product storage identity, compatible timing flag and one-time migration startup/report hooks.");
    }
    static void VerifyClientStorage(string root) {
        var data=Path.Combine(Path.GetFullPath(root),"King's Raid_Data");
        VerifyClientStorageFiles(Path.Combine(data,"globalgamemanagers"),Path.Combine(data,"Managed","SprkAutomation.dll"),Path.Combine(data,"Managed","Assembly-CSharp.dll"));
    }
    static void InstallClientStorage(string root,bool stageOnly) {
        root=Path.GetFullPath(root);var data=Path.Combine(root,"King's Raid_Data");var managed=Path.Combine(data,"Managed");
        var stage=Path.GetFullPath(Path.Combine("target/client-storage",Guid.NewGuid().ToString("N")));Directory.CreateDirectory(stage);
        string Resource(string name){using var stream=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher."+name)!;using var reader=new StreamReader(stream);return reader.ReadToEnd();}
        var script=Path.Combine(stage,"isolate_client_storage.py");File.WriteAllText(script,Resource("isolate_client_storage.py"));
        var fingerprint=Path.Combine(stage,"original-player-settings.bin");File.WriteAllBytes(fingerprint,OriginalPlayerSettings());
        var settings=Path.Combine(data,"globalgamemanagers");var staged=Path.Combine(stage,"globalgamemanagers");
        var start=new System.Diagnostics.ProcessStartInfo(Environment.GetEnvironmentVariable("SPRK_PYTHON") ?? (OperatingSystem.IsWindows()?"python":"python3")) {UseShellExecute=false,CreateNoWindow=true};
        foreach(var arg in new[]{script,settings,staged,fingerprint})start.ArgumentList.Add(arg);
        using(var process=System.Diagnostics.Process.Start(start)!){process.WaitForExit();if(process.ExitCode!=0)throw new InvalidOperationException("Storage identity asset conversion failed.");}
        bool gameChanged=InstallAutomation(root,true);
        var game=Path.Combine(managed,"Assembly-CSharp.dll");var helper=Path.Combine(managed,"SprkAutomation.dll");
        VerifyClientStorageFiles(staged,helper+".staged",gameChanged?game+".staged":game);
        // A stable staging path is used by the isolated native runtime probe.
        var stable=Path.GetFullPath("target/client-storage/staged/King's Raid_Data");Directory.CreateDirectory(Path.Combine(stable,"Managed"));
        File.Copy(staged,Path.Combine(stable,"globalgamemanagers"),true);File.Copy(helper+".staged",Path.Combine(stable,"Managed","SprkAutomation.dll"),true);
        File.Copy(gameChanged?game+".staged":game,Path.Combine(stable,"Managed","Assembly-CSharp.dll"),true);
        if(stageOnly){Console.WriteLine("Staged isolated client storage; installed files unchanged.");return;}
        foreach(var process in System.Diagnostics.Process.GetProcesses())using(process)if(UsesClientFiles(process,root))throw new InvalidOperationException("Storage patch staged. Close game clients and shared-asset workers before installation.");
        var backup=Path.Combine(stage,"backups");Directory.CreateDirectory(backup);
        foreach(var (source,destination) in new[]{(staged,settings),(helper+".staged",helper)}){File.Copy(destination,Path.Combine(backup,Path.GetFileName(destination)));File.Copy(source,destination,true);}
        if(gameChanged){File.Copy(game,Path.Combine(backup,"Assembly-CSharp.dll"));File.Copy(game+".staged",game,true);}
        File.Delete(helper+".staged");if(gameChanged)File.Delete(game+".staged");
        VerifyClientStorage(root);Console.WriteLine("Installed isolated sprk client storage and one-time migration.");
    }
    static void TestClientStorage() {
        string Resource(string name){using var stream=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher."+name)!;using var reader=new StreamReader(stream);return reader.ReadToEnd();}
        var refs=((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(p=>Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(p));
        var compilation=Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create("StorageTests-"+Guid.NewGuid().ToString("N"),new[]{Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(Resource("StorageMigration.cs.txt")),Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(Resource("StorageMigrationTests.cs.txt"))},refs,new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary));
        using var output=new MemoryStream();var emitted=compilation.Emit(output);if(!emitted.Success)throw new InvalidOperationException(string.Join("\n",emitted.Diagnostics));
        System.Reflection.Assembly.Load(output.ToArray()).GetType("StorageMigrationTestRunner")!.GetMethod("Run")!.Invoke(null,null);
        foreach(var originalFlag in new[]{0,1})foreach(var logging in new[]{0,1}) {
            var original=OriginalPlayerSettings();original[648]=(byte)originalFlag;original[410]=(byte)logging;var isolated=IsolatedPlayerSettings(original);
            int offset=FindFrameTimingSettings(isolated,out bool enabled);if(offset!=640 || enabled!=(originalFlag==1))throw new InvalidOperationException("Isolated timing flag mismatch.");
            if(isolated[402]!=logging)throw new InvalidOperationException("Storage conversion changed player logging.");
        }
        Console.WriteLine("PASS: one-time storage migration, existing data preservation, preference scope, no future reimport, tombstone recovery, partial failure, concurrency and both timing fingerprints.");
    }

}
