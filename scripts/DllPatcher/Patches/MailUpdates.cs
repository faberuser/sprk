using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program {
    static void TestMailUpdates() {
        string Resource(string name) {
            using var input=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher."+name)!;
            using var reader=new StreamReader(input);return reader.ReadToEnd();
        }
        var references=((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("MailUpdatesTests-"+Guid.NewGuid().ToString("N"),
            new[] {CSharpSyntaxTree.ParseText(Resource("MailUpdates.cs.txt")),CSharpSyntaxTree.ParseText(Resource("MailUpdatesTests.cs.txt"))},references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var output=new MemoryStream();var result=compilation.Emit(output);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics));
        System.Reflection.Assembly.Load(output.ToArray()).GetType("MailUpdatesTestRunner")!.GetMethod("Run")!.Invoke(null,null);
        Console.WriteLine("Passed mailbox scheduling: login, socket wake-up, burst debounce, in-flight guard, sixty-second fallback, live inbox and reconnect recovery.");
    }
    static void PatchMailUpdates(ModuleDefinition module, ModuleDefinition helper) {
        var type = helper.GetType("SprkMailUpdates");
        MethodReference Hook(string name) => module.ImportReference(type.Methods.Single(m => m.Name == name));
        var lobby = module.GetType("NGame2.NUI.NManager.NLobby.LobbyManagement");
        var check = lobby.Methods.Single(m => m.Name == "CheckNewMailArrived");
        check.Body = new MethodBody(check);
        var il = check.Body.GetILProcessor();
        il.Append(il.Create(OpCodes.Ldarg_0));
        il.Append(il.Create(OpCodes.Call, Hook("Tick")));
        il.Append(il.Create(OpCodes.Ret));
        var arrived = module.GetType("NGame2.NMessageLog.NProtocol.NewMailArrivedProtocolHandler")
            .Methods.Single(m => m.Name == "Run");
        if (!arrived.Body.Instructions.Any(i => i.Operand is MethodReference m && m.DeclaringType.Name == "SprkMailUpdates" && m.Name == "Arrived")) {
            foreach (var ret in arrived.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray()) {
                ret.OpCode = OpCodes.Call; ret.Operand = Hook("Arrived");
                arrived.Body.GetILProcessor().InsertAfter(ret, Instruction.Create(OpCodes.Ret));
            }
        }
        var callback = module.GetTypes().SelectMany(t => t.Methods).Single(m => m.HasBody
            && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.FullName == "NShared.CheckNewMail.Response");
        if (!callback.Body.Instructions.Any(i => i.Operand is MethodReference m && m.DeclaringType.Name == "SprkMailUpdates" && m.Name == "Finished")) {
            var first = callback.Body.Instructions[0];
            var arg = callback.IsStatic ? OpCodes.Ldarg_0 : OpCodes.Ldarg_1;
            callback.Body.GetILProcessor().InsertBefore(first, Instruction.Create(arg));
            callback.Body.GetILProcessor().InsertBefore(first, Instruction.Create(OpCodes.Call, Hook("Finished")));
        }
    }

    static void VerifyMailUpdates(string clientRoot) {
        var managed = Path.Combine(Path.GetFullPath(clientRoot), "King's Raid_Data", "Managed");
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"));
        using var helper = AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkMailUpdates.dll"));
        var calls = assembly.MainModule.GetTypes().SelectMany(t => t.Methods).Where(m => m.HasBody)
            .SelectMany(m => m.Body.Instructions).Select(i => i.Operand).OfType<MethodReference>()
            .Where(m => m.DeclaringType.Name == "SprkMailUpdates").ToArray();
        foreach (var name in new[] { "Tick", "Finished" })
            if (calls.Count(m => m.Name == name) != 1) throw new InvalidOperationException("Mailbox hook missing or duplicated: " + name);
        if (!calls.Any(m => m.Name == "Arrived")) throw new InvalidOperationException("Mailbox socket hook missing.");
        var tick = helper.MainModule.GetType("SprkMailUpdates").Methods.Single(m => m.Name == "Tick");
        if (!tick.Body.Instructions.Any(i => i.OpCode == OpCodes.Ldc_R4 && Equals(i.Operand,60f)))
            throw new InvalidOperationException("Mailbox fallback interval missing.");
        Console.WriteLine("Verified mailbox socket wake-up, sixty-second fallback, completion guard and live inbox refresh.");
    }

    static void InstallMailUpdates(string clientRoot, bool stageOnly) {
        clientRoot = Path.GetFullPath(clientRoot);
        var managed = Path.Combine(clientRoot,"King's Raid_Data","Managed");
        var original = Path.Combine(managed,"Assembly-CSharp.dll");
        using var input = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.MailUpdates.cs.txt")!;
        using var reader = new StreamReader(input);
        var references = Directory.GetFiles(managed,"*.dll").Where(p => Path.GetFileName(p) != "SprkMailUpdates.dll")
            .Select(p => MetadataReference.CreateFromFile(p));
        var compilation = CSharpCompilation.Create("SprkMailUpdates", new[] { CSharpSyntaxTree.ParseText(reader.ReadToEnd()) }, references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));
        using var compiled = new MemoryStream();
        var result = compilation.Emit(compiled);
        if (!result.Success) throw new InvalidOperationException(string.Join("\n",result.Diagnostics));
        var stageRoot = Path.GetFullPath("target/mail-delivery-fix/staged");
        var staged = Path.Combine(stageRoot,"King's Raid_Data","Managed");
        Directory.CreateDirectory(staged);
        using (var resolver = new DefaultAssemblyResolver()) {
            resolver.AddSearchDirectory(managed);
            using var assembly = AssemblyDefinition.ReadAssembly(original,new ReaderParameters { AssemblyResolver = resolver });
            using var helper = AssemblyDefinition.ReadAssembly(new MemoryStream(compiled.ToArray()));
            PatchMailUpdates(assembly.MainModule,helper.MainModule);
            assembly.Write(Path.Combine(staged,"Assembly-CSharp.dll"));
            File.WriteAllBytes(Path.Combine(staged,"SprkMailUpdates.dll"),compiled.ToArray());
        }
        VerifyMailUpdates(stageRoot);
        if (!stageOnly) {
            foreach (var process in System.Diagnostics.Process.GetProcesses()) using (process) {
                if (process.ProcessName == "King's Raid") throw new InvalidOperationException("Close game clients before installing the verified mailbox fix.");
            }
            var backup = Path.GetFullPath(Path.Combine("target/mail-delivery-fix/backups",DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")));
            Directory.CreateDirectory(backup);
            foreach (var name in new[] { "Assembly-CSharp.dll", "SprkMailUpdates.dll" }) {
                var destination = Path.Combine(managed,name);
                if (File.Exists(destination)) File.Copy(destination,Path.Combine(backup,name));
                File.Copy(Path.Combine(staged,name),destination,true);
            }
            VerifyMailUpdates(clientRoot);
        }
        Console.WriteLine(stageOnly ? "Staged mailbox delivery fix; installed files unchanged." : "Installed mailbox delivery fix. Restart the game.");
    }
}
