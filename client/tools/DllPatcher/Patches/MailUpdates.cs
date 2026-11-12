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
        TestMailClaimAll();
        Console.WriteLine("Passed mailbox scheduling: login, socket wake-up, burst debounce, in-flight guard, sixty-second fallback, live inbox and reconnect recovery.");
    }
    static void TestMailClaimAll() {
        string Resource(string name) {
            using var input=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher."+name)!;
            using var reader=new StreamReader(input);return reader.ReadToEnd();
        }
        var references=((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("MailClaimAllTests-"+Guid.NewGuid().ToString("N"),
            new[] {CSharpSyntaxTree.ParseText(Resource("MailClaimAll.cs.txt")),CSharpSyntaxTree.ParseText(Resource("MailClaimAllTests.cs.txt"))},references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var output=new MemoryStream();var result=compilation.Emit(output);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics));
        System.Reflection.Assembly.Load(output.ToArray()).GetType("MailClaimAllTestRunner")!.GetMethod("Run")!.Invoke(null,null);
        Console.WriteLine("PASS: mailbox drain over 1500 mails, capacity stop, one summary, duplicate/in-flight guards, cancellation, session change, failure and timeout.");
    }
    static void PatchMailUpdates(ModuleDefinition module, ModuleDefinition helper) {
        var type = helper.GetType("SprkMailUpdates");
        MethodReference Hook(string name) => module.ImportReference(type.Methods.Single(m => m.Name == name));
        MethodReference ClaimHook(string name) => module.ImportReference(helper.GetType("SprkMailClaimAll").Methods.Single(m=>m.Name==name));
        var manager=module.GetType("NGame2.NUI.NManager.MailManagement");
        var receive=manager.Methods.Single(m=>m.Name=="OnReceiveAll_Real");
        receive.Body=new MethodBody(receive);
        var receiveIl=receive.Body.GetILProcessor();
        receiveIl.Emit(OpCodes.Ldarg_0);receiveIl.Emit(OpCodes.Ldarg_1);receiveIl.Emit(OpCodes.Ldarg_2);
        receiveIl.Emit(OpCodes.Call,ClaimHook("Start"));receiveIl.Emit(OpCodes.Ret);
        var window=module.GetType("NGame2.NUI.NWindow.MailListPopup");
        foreach(var (name,hook) in new[]{("Init","Caption"),("OnClosing","Cancel")}) {
            var method=window.Methods.Single(m=>m.Name==name);
            if(method.Body.Instructions.Any(i=>i.Operand is MethodReference m&&m.DeclaringType.Name=="SprkMailClaimAll"&&m.Name==hook))continue;
            var hookIl=method.Body.GetILProcessor();
            if(hook=="Cancel") {
                var first=method.Body.Instructions[0];
                hookIl.InsertBefore(first,hookIl.Create(OpCodes.Ldarg_0));hookIl.InsertBefore(first,hookIl.Create(OpCodes.Call,ClaimHook(hook)));
            } else foreach(var ret in method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=OpCodes.Ldarg_0;ret.Operand=null;
                var call=hookIl.Create(OpCodes.Call,ClaimHook(hook));hookIl.InsertAfter(ret,call);hookIl.InsertAfter(call,hookIl.Create(OpCodes.Ret));
            }
        }
        var encode=module.GetType("JM_NShared_ReceiveAllMail_Request").Methods.Single(m=>m.Name=="EncodeJsonObject");
        if(!encode.Body.Instructions.Any(i=>i.Operand is MethodReference m&&m.DeclaringType.Name=="SprkMailClaimAll"))
            foreach(var ret in encode.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=OpCodes.Call;ret.Operand=ClaimHook("Encode");
                encode.Body.GetILProcessor().InsertAfter(ret,Instruction.Create(OpCodes.Ret));
            }
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
        foreach(var (typeName,methodName,hook) in new[]{
            ("NGame2.NUI.NManager.MailManagement","OnReceiveAll_Real","Start"),
            ("NGame2.NUI.NWindow.MailListPopup","Init","Caption"),
            ("NGame2.NUI.NWindow.MailListPopup","OnClosing","Cancel"),
            ("JM_NShared_ReceiveAllMail_Request","EncodeJsonObject","Encode")}) {
            var method=assembly.MainModule.GetType(typeName).Methods.Single(m=>m.Name==methodName);
            if(!method.Body.Instructions.Any(i=>i.Operand is MethodReference m&&m.DeclaringType.Name=="SprkMailClaimAll"&&m.Name==hook))
                throw new InvalidOperationException("Missing automatic mail claim hook: "+hook);
        }
        var claimCalls=helper.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody)
            .SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand).OfType<MethodReference>().Select(m=>m.Name).ToArray();
        foreach(var name in new[]{"Request","ApplyReceiveMail","OnReceiveAll","WindowOpenedUI3DUI","OpenPopup"})
            if(!claimCalls.Contains(name))throw new InvalidOperationException("Incomplete automatic mail claim helper: "+name);
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
        using var claimInput=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.MailClaimAll.cs.txt")!;
        using var claimReader=new StreamReader(claimInput);
        var references = Directory.GetFiles(managed,"*.dll").Where(p => Path.GetFileName(p) != "SprkMailUpdates.dll")
            .Select(p => MetadataReference.CreateFromFile(p));
        var compilation = CSharpCompilation.Create("SprkMailUpdates", new[] { CSharpSyntaxTree.ParseText(reader.ReadToEnd()), CSharpSyntaxTree.ParseText(claimReader.ReadToEnd()) }, references,
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
            int count=assembly.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).Sum(m=>m.Body.Instructions.Count);
            PatchMailUpdates(assembly.MainModule,helper.MainModule);
            if(count!=assembly.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).Sum(m=>m.Body.Instructions.Count))
                throw new InvalidOperationException("Mail claim patch is not idempotent.");
            assembly.Write(Path.Combine(staged,"Assembly-CSharp.dll"));
            File.WriteAllBytes(Path.Combine(staged,"SprkMailUpdates.dll"),compiled.ToArray());
        }
        VerifyMailUpdates(stageRoot);
        if (!stageOnly) {
            foreach (var process in System.Diagnostics.Process.GetProcesses()) using (process) {
                if (UsesClientFiles(process,clientRoot)) throw new InvalidOperationException("Close game clients and shared-asset workers before installing the verified mailbox fix.");
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
