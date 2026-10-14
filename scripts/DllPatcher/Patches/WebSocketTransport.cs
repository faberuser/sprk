using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static MethodDefinition SocketClosedCallback(TypeDefinition connector)
    {
        return connector.NestedTypes.SelectMany(t=>t.Methods).Single(m=>m.HasBody &&
            m.Parameters.Count==2 && m.Parameters[1].ParameterType.FullName=="System.EventArgs" &&
            m.Body.Instructions.Any(i=>i.Operand is MethodReference r &&
                ((r.DeclaringType.FullName==connector.FullName && r.Name=="Reserve") ||
                 (r.DeclaringType.Name=="SprkWebSocketTransport" && r.Name=="SessionClosed"))));
    }
    static void VerifyWebSocketLifecycle(string clientRoot)
    {
        var path=Path.Combine(Path.GetFullPath(clientRoot),"King's Raid_Data","Managed","Assembly-CSharp.dll");
        using var game=AssemblyDefinition.ReadAssembly(path);
        var callback=SocketClosedCallback(game.MainModule.GetType("NVespa.NSocket.SocketConnector"));
        if(callback.Body.Instructions.Count(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="SprkWebSocketTransport" && r.Name=="SessionClosed")!=1 ||
           callback.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.Name=="Close"))
            throw new InvalidOperationException("Socket Closed must use the session-scoped guard.");
        Console.WriteLine("Verified socket close callback cannot close a replacement session.");
    }
    static void InstallWebSockets(string clientRoot, bool stageOnly, string? sourcePath = null)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        sourcePath ??= dll;
        using var source = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.WebSocketTransport.cs.txt")!;
        using var reader = new StreamReader(source);
        var references = Directory.GetFiles(managed, "*.dll")
            .Where(path => Path.GetFileName(path) != "SprkWebSocketTransport.dll")
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create("SprkWebSocketTransport",
            new[] { CSharpSyntaxTree.ParseText(reader.ReadToEnd()) }, references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));
        using var compiled = new MemoryStream();
        var emitted = compilation.Emit(compiled);
        if (!emitted.Success) throw new InvalidOperationException(string.Join("\n", emitted.Diagnostics));
        compiled.Position = 0;
        using var helper = AssemblyDefinition.ReadAssembly(compiled);
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using var assembly = AssemblyDefinition.ReadAssembly(sourcePath, new ReaderParameters { AssemblyResolver = resolver, InMemory = true });
        var module = assembly.MainModule;
        var connector = module.Types.Single(t => t.FullName == "NVespa.NSocket.SocketConnector");
        var original = connector.Methods.SingleOrDefault(m => m.Name == "SprkConnectTcpCoroutine");
        if (original == null)
        {
            original = connector.Methods.Single(m => m.Name == "ConnectSocketCoroutine");
            original.Name = "SprkConnectTcpCoroutine";
            var wrapper = new MethodDefinition("ConnectSocketCoroutine", original.Attributes, original.ReturnType);
            foreach (var parameter in original.Parameters)
                wrapper.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, parameter.ParameterType));
            connector.Methods.Add(wrapper);
            // Existing calls must go through the adapter; the old coroutine
            // remains available by reflection for native lifecycle handling.
            foreach (var method in connector.Methods.Where(m => m.HasBody && m != wrapper))
                foreach (var instruction in method.Body.Instructions)
                    if (instruction.Operand is MethodReference reference && reference.Resolve() == original)
                        instruction.Operand = wrapper;
        }
        var target = connector.Methods.Single(m => m.Name == "ConnectSocketCoroutine");
        target.Body = new MethodBody(target);
        var il = target.Body.GetILProcessor();
        il.Append(Instruction.Create(OpCodes.Ldarg_0));
        foreach (var parameter in target.Parameters) il.Append(Instruction.Create(OpCodes.Ldarg, parameter));
        var connect = helper.MainModule.Types.Single(t => t.Name == "SprkWebSocketTransport").Methods.Single(m => m.Name == "Connect");
        il.Append(Instruction.Create(OpCodes.Call, module.ImportReference(connect)));
        il.Append(Instruction.Create(OpCodes.Ret));
        var closed=SocketClosedCallback(connector);
        var owner=closed.DeclaringType.Fields.Single(f=>f.FieldType.FullName==connector.FullName);
        closed.Body=new MethodBody(closed);
        var closedIl=closed.Body.GetILProcessor();
        closedIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        closedIl.Append(Instruction.Create(OpCodes.Ldfld,owner));
        closedIl.Append(Instruction.Create(OpCodes.Ldarg_1));
        var guard=helper.MainModule.Types.Single(t=>t.Name=="SprkWebSocketTransport").Methods.Single(m=>m.Name=="SessionClosed");
        closedIl.Append(Instruction.Create(OpCodes.Call,module.ImportReference(guard)));
        closedIl.Append(Instruction.Create(OpCodes.Ret));
        PatchChatSession(module);
        var staged = dll + ".websocket-staged";
        assembly.Write(staged);
        File.WriteAllBytes(Path.Combine(managed, "SprkWebSocketTransport.dll.staged"), compiled.ToArray());
        if (!stageOnly)
        {
            var suffix = ".backup-websocket-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff");
            File.Copy(dll, dll + suffix);
            var helperPath = Path.Combine(managed, "SprkWebSocketTransport.dll");
            if (File.Exists(helperPath)) File.Copy(helperPath, helperPath + suffix);
            // Install the dependency before any patched call can reference it.
            File.Move(Path.Combine(managed, "SprkWebSocketTransport.dll.staged"), helperPath, true);
            File.Move(staged, dll, true);
        }
        Console.WriteLine(stageOnly ? "Staged WebSocket client transport" : "Installed WebSocket client transport");
    }
}
