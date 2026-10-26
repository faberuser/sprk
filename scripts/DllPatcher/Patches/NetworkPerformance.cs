using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program {
    static void InstallNetworkRuntimeProbe(string clientRoot) {
        var root=Path.GetFullPath(clientRoot);
        var allowed=Path.GetFullPath("target/network-performance")+Path.DirectorySeparatorChar;
        if(!root.StartsWith(allowed,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Network runtime probes require an isolated copy under target/network-performance.");
        var managed=Path.Combine(root,"King's Raid_Data","Managed");
        using var stream=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.NetworkPerformanceProbe.cs.txt")!;
        using var reader=new StreamReader(stream);
        var compilation=CSharpCompilation.Create("SprkNetworkProbe",new[]{CSharpSyntaxTree.ParseText(reader.ReadToEnd())},
            Directory.GetFiles(managed,"*.dll").Select(p=>MetadataReference.CreateFromFile(p)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var output=new MemoryStream();var result=compilation.Emit(output);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics));
        File.WriteAllBytes(Path.Combine(managed,"SprkNetworkProbe.dll"),output.ToArray());
        output.Position=0;using var helper=AssemblyDefinition.ReadAssembly(output);
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");using var assembly=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver});
        var awake=assembly.MainModule.GetType("UICamera").Methods.Single(m=>m.Name=="Awake");
        awake.Body.GetILProcessor().InsertBefore(awake.Body.Instructions[0],Instruction.Create(OpCodes.Call,
            assembly.MainModule.ImportReference(helper.MainModule.GetType("SprkNetworkProbe").Methods.Single(m=>m.Name=="Ensure"))));
        assembly.Write(dll+".probe");assembly.Dispose();File.Move(dll+".probe",dll,true);
    }
    static void InstallNetworkPerformance(string clientRoot, bool stageOnly) {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        using var resource = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.NetworkPerformance.cs.txt")!;
        using var reader = new StreamReader(resource);
        var compilation = CSharpCompilation.Create("SprkNetworkPerformance", new[]{CSharpSyntaxTree.ParseText(reader.ReadToEnd())},
            Directory.GetFiles(managed,"*.dll").Where(p=>Path.GetFileName(p)!="SprkNetworkPerformance.dll").Select(p=>MetadataReference.CreateFromFile(p)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel:OptimizationLevel.Release));
        using var output = new MemoryStream(); var result = compilation.Emit(output);
        if (!result.Success) throw new InvalidOperationException(string.Join("\n",result.Diagnostics));
        output.Position=0; using var helper=AssemblyDefinition.ReadAssembly(output);
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var assembly=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver});
        var module=assembly.MainModule;
        var requester=module.GetType("NVespa.NWeb.WebServiceRequester");
        var hooks=helper.MainModule.GetType("SprkNetworkPerformance");
        MethodReference Hook(string name)=>module.ImportReference(hooks.Methods.Single(m=>m.Name==name));
        void Prefix(MethodDefinition method,string hook,params Instruction[] args) {
            if(method.Body.Instructions.Any(i=>i.Operand is MethodReference m&&m.DeclaringType.Name==hooks.Name&&m.Name==hook))return;
            var first=method.Body.Instructions[0];var il=method.Body.GetILProcessor();
            foreach(var arg in args)il.InsertBefore(first,arg);
            il.InsertBefore(first,Instruction.Create(OpCodes.Call,Hook(hook)));
        }
        Prefix(module.GetType("NVespa.NWeb.WebServiceRequest").Methods.Single(m=>m.Name=="CreateRequest"),"Started",Instruction.Create(OpCodes.Ldarg_0));
        Prefix(requester.Methods.Single(m=>m.Name=="ProcessWebResponseThreaded"),"BeginResponse",Instruction.Create(OpCodes.Ldarg_2));
        Prefix(module.GetType("NVespa.WwwText").Methods.Single(m=>m.Name=="OnComplete"),"TransportComplete",Instruction.Create(OpCodes.Ldarg_1));
        // Both free and paid restocks already carry purchase counters. Apply
        // them to the separate native purchase-count cache before opening UI.
        var facade=module.GetType("NGame2.NUI.NWindow.NShop.ShopFacade");
        foreach(var method in facade.NestedTypes.SelectMany(t=>t.Methods).Where(m=>m.HasBody)) {
            var get=method.Body.Instructions.FirstOrDefault(i=>i.Operand is MethodReference m&&m.Name=="get_ShopItems"
                && (m.DeclaringType.FullName=="NShared.GetShopList.Response" || m.DeclaringType.FullName=="NShared.RequestShopList.Response"));
            if(get==null || method.Body.Instructions.Any(i=>i.Operand is MethodReference m&&m.DeclaringType.Name==hooks.Name&&m.Name=="ShopListApplied"))continue;
            var set=method.Body.Instructions.SkipWhile(i=>i!=get).First(i=>i.Operand is MethodReference m&&m.Name=="Set");
            var shopResponse=method.Parameters.Single(p=>p.ParameterType.FullName==((MethodReference)get.Operand).DeclaringType.FullName);
            var next=set.Next;var il=method.Body.GetILProcessor();
            foreach(var i in new[]{Instruction.Create(OpCodes.Ldarg,shopResponse),Instruction.Create(OpCodes.Callvirt,(MethodReference)get.Operand),Instruction.Create(OpCodes.Call,Hook("ShopListApplied"))})il.InsertBefore(next,i);
        }
        var stamina=module.GetType("NGame2.NAccount.UserStaminaManager");
        var applyClock=stamina.Methods.Single(m=>m.Name=="ApplyExpectedRechargeTime");
        if(!applyClock.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name==hooks.Name && m.Name=="SetStaminaClock")) {
            var first=applyClock.Body.Instructions[0];var il=applyClock.Body.GetILProcessor();
            foreach(var i in new[]{Instruction.Create(OpCodes.Ldarg_0),Instruction.Create(OpCodes.Ldarg_1),Instruction.Create(OpCodes.Ldarg_2),
                Instruction.Create(OpCodes.Call,module.ImportReference(hooks.Methods.Single(m=>m.Name=="SetStaminaClock")))})il.InsertBefore(first,i);
        }
        var automatic=stamina.Methods.Single(m=>m.Name=="IsUpdateStamina");
        if(!automatic.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name==hooks.Name && m.Name=="HasStaminaClock")) {
            var first=automatic.Body.Instructions[0];var il=automatic.Body.GetILProcessor();
            foreach(var i in new[]{Instruction.Create(OpCodes.Ldarg_0),Instruction.Create(OpCodes.Ldarg_1),
                Instruction.Create(OpCodes.Call,module.ImportReference(hooks.Methods.Single(m=>m.Name=="HasStaminaClock"))),
                Instruction.Create(OpCodes.Brtrue,first),Instruction.Create(OpCodes.Ldc_I4_0),Instruction.Create(OpCodes.Ret)})il.InsertBefore(first,i);
        }
        var prepare=requester.Methods.Single(m=>m.Name=="RequestInternal");
        if(!prepare.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name==hooks.Name)) {
            // This local holds the fully constructed request before it is queued.
            var variable=prepare.Body.Variables.Single(v=>v.VariableType is GenericInstanceType g && g.ElementType.FullName=="NVespa.NWeb.WebServiceRequest`2");
            var store=prepare.Body.Instructions.Single(i=>(i.OpCode==OpCodes.Stloc || i.OpCode==OpCodes.Stloc_S) && i.Operand==variable);
            var next=store.Next; var il=prepare.Body.GetILProcessor();
            foreach(var i in new[]{Instruction.Create(OpCodes.Ldloc,variable),Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call,requester.Methods.Single(m=>m.Name=="get_SessionKey")),
                Instruction.Create(OpCodes.Call,module.ImportReference(hooks.Methods.Single(m=>m.Name=="Prepare")))}) il.InsertBefore(next,i);
        }
        var response=requester.Methods.Single(m=>m.Name=="ProcessWebResponseThreaded");
        // Upgrade the initial parse-only hook without leaving references to an
        // obsolete helper signature in previously patched installations.
        var old=response.Body.Instructions.SingleOrDefault(i=>i.Operand is MethodReference m && m.DeclaringType.Name==hooks.Name && m.Name=="Observe");
        if(old!=null) {
            var removal=new[]{old.Previous.Previous.Previous,old.Previous.Previous,old.Previous,old,old.Next};
            foreach(var i in removal)response.Body.GetILProcessor().Remove(i);
        }
        if(!response.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name==hooks.Name && m.Name=="Capture")) {
            var parse=response.Body.Instructions.Single(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="JsonMarshaler" && m.Name=="Parse");
            var il=response.Body.GetILProcessor();
            var textField=(FieldReference)parse.Previous.Operand;
            var resultField=(FieldReference)parse.Next.Operand;
            var next=parse.Next.Next; // parsed response has been stored successfully
            foreach(var i in new[]{Instruction.Create(OpCodes.Ldloc_0),Instruction.Create(OpCodes.Ldfld,textField),
                Instruction.Create(OpCodes.Ldarg_2),Instruction.Create(OpCodes.Ldloc_0),Instruction.Create(OpCodes.Ldfld,resultField),
                Instruction.Create(OpCodes.Box,resultField.FieldType),
                Instruction.Create(OpCodes.Call,module.ImportReference(hooks.Methods.Single(m=>m.Name=="Capture")))})il.InsertBefore(next,i);
        }
        var callback=requester.NestedTypes.SelectMany(t=>t.Methods).Single(m=>m.Name=="<ProcessWebResponseThreaded>b__0");
        if(!callback.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name==hooks.Name && m.Name=="ProgressionApplied")) {
            var get=callback.Body.Instructions.Single(i=>i.Operand is MethodReference m && m.Name=="get_OnBaseResponse");
            var fire=callback.Body.Instructions.SkipWhile(i=>i!=get).First(i=>i.Operand is MethodReference m && m.Name=="fire");
            var resultField=(FieldReference)get.Next.Next.Operand;
            var next=fire.Next;var il=callback.Body.GetILProcessor();
            foreach(var i in new[]{Instruction.Create(OpCodes.Ldarg_0),Instruction.Create(OpCodes.Ldfld,resultField),Instruction.Create(OpCodes.Box,resultField.FieldType),
                Instruction.Create(OpCodes.Call,module.ImportReference(hooks.Methods.Single(m=>m.Name=="ProgressionApplied")))})il.InsertBefore(next,i);
        }
        var lobby=module.GetType("NGame2.NScene.NLobbyScenePrerequisites.RequestEnterLobbyWrapper");
        var handle=lobby.Methods.Single(m=>m.Name=="HandleResponseInternal");
        if(!handle.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name==hooks.Name && m.Name=="CatalogApplied")) {
            var set=handle.Body.Instructions.Single(i=>i.Operand is MethodReference m && m.Name=="SetNewPayShopProductInfo");
            var next=set.Next;var il=handle.Body.GetILProcessor();
            foreach(var i in new[]{Instruction.Create(OpCodes.Ldarg_0),Instruction.Create(OpCodes.Ldfld,lobby.Fields.Single(f=>f.Name=="_response")),
                Instruction.Create(OpCodes.Call,module.ImportReference(hooks.Methods.Single(m=>m.Name=="CatalogApplied")))})il.InsertBefore(next,i);
        }
        var staged=dll+".network-staged";
        var helperPath=Path.Combine(managed,"SprkNetworkPerformance.dll");
        assembly.Write(staged);File.WriteAllBytes(helperPath+".staged",output.ToArray());
        VerifyNetworkPerformanceFiles(staged,helperPath+".staged");
        if(!stageOnly) {
            foreach(var process in System.Diagnostics.Process.GetProcesses())using(process)
                if(UsesClientFiles(process,Path.GetFullPath(clientRoot)))throw new InvalidOperationException("Close game clients before installing network performance changes.");
            File.Copy(dll,dll+".before-network-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
            assembly.Dispose();File.Move(staged,dll,true);File.Move(helperPath+".staged",helperPath,true);
        }
    }

    static void VerifyNetworkPerformanceFile(string clientRoot) {
        var managed=Path.Combine(Path.GetFullPath(clientRoot),"King's Raid_Data","Managed");
        VerifyNetworkPerformanceFiles(Path.Combine(managed,"Assembly-CSharp.dll"),Path.Combine(managed,"SprkNetworkPerformance.dll"));
    }
    static void VerifyNetworkPerformanceFiles(string assemblyPath,string helperPath) {
        using var assembly=AssemblyDefinition.ReadAssembly(assemblyPath);
        using var helper=AssemblyDefinition.ReadAssembly(helperPath);
        var requester=assembly.MainModule.GetType("NVespa.NWeb.WebServiceRequester");
        foreach(var (method,hook) in new[]{("RequestInternal","Prepare"),("ProcessWebResponseThreaded","Capture")}) {
            var instructions=requester.Methods.Single(m=>m.Name==method).Body.Instructions;
            if(instructions.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkNetworkPerformance" && m.Name==hook)!=1)
                throw new InvalidOperationException("Missing or duplicated network hook: "+hook);
        }
        var lobby=assembly.MainModule.GetType("NShared.EnterLobby.Response");
        foreach(var name in new[]{"Heroes","Items","UserInfo"})
            if(lobby.Properties.Concat(assembly.MainModule.GetType("NShared.Response").Properties).Any(p=>p.Name==name))
                throw new InvalidOperationException("Compact lobby would omit a native field: "+name);
        var lobbyHandler=assembly.MainModule.GetType("NGame2.NScene.NLobbyScenePrerequisites.RequestEnterLobbyWrapper")
            .Methods.Single(m=>m.Name=="HandleResponseInternal");
        var catalog=lobbyHandler.Body.Instructions.First(i=>i.Operand is MethodReference m && m.Name=="get_PayShopProductInfos");
        if(catalog.Next.OpCode!=OpCodes.Brfalse && catalog.Next.OpCode!=OpCodes.Brfalse_S)
            throw new InvalidOperationException("Native lobby must preserve its catalog when PayShopProductInfos is absent.");
        foreach(var name in new[]{"Prepare","Capture","ProgressionApplied","CatalogApplied","SetStaminaClock","HasStaminaClock","Started","BeginResponse","TransportComplete","ShopListApplied"})
            if(!helper.MainModule.GetType("SprkNetworkPerformance").Methods.Any(m=>m.Name==name))throw new InvalidOperationException("Missing network helper: "+name);
        var responseCode=requester.Methods.Single(m=>m.Name=="ProcessWebResponseThreaded").Body.Instructions;
        var parseIndex=responseCode.ToList().FindIndex(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="JsonMarshaler" && m.Name=="Parse");
        var observeIndex=responseCode.ToList().FindIndex(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkNetworkPerformance" && m.Name=="Capture");
        if(observeIndex<=parseIndex)throw new InvalidOperationException("Snapshot acknowledgement must follow successful native parsing.");
        foreach(var hook in new[]{"ProgressionApplied","CatalogApplied","SetStaminaClock","HasStaminaClock"})
            if(assembly.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions)
                .Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkNetworkPerformance" && m.Name==hook)!=1)
                throw new InvalidOperationException("Missing or duplicated native application hook: "+hook);
        var allCalls=assembly.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions)
            .Select(i=>i.Operand).OfType<MethodReference>().Where(m=>m.DeclaringType.Name=="SprkNetworkPerformance").ToArray();
        foreach(var hook in new[]{"Started","BeginResponse","TransportComplete"})
            if(allCalls.Count(m=>m.Name==hook)!=1)throw new InvalidOperationException("Missing or duplicate network timing hook: "+hook);
        if(allCalls.Count(m=>m.Name=="ShopListApplied")!=2)throw new InvalidOperationException("Both shop restock callbacks must reuse purchase counters.");
        var stamina=assembly.MainModule.GetType("NGame2.NAccount.UserStaminaManager");
        foreach(var (name,hook) in new[]{("ApplyExpectedRechargeTime","SetStaminaClock"),("IsUpdateStamina","HasStaminaClock")})
            if(!stamina.Methods.Single(m=>m.Name==name).Body.Instructions.Take(4).Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkNetworkPerformance" && m.Name==hook))
                throw new InvalidOperationException("Stamina clock hook must precede native processing: "+name);
        Console.WriteLine("Verified request acknowledgements, post-parse response hook and unused legacy lobby fields.");
    }

    static void TestNetworkPerformance() {
        string Resource(string name) {using var input=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher."+name)!;using var reader=new StreamReader(input);return reader.ReadToEnd();}
        var compilation=CSharpCompilation.Create("NetworkPerformanceTests-"+Guid.NewGuid().ToString("N"),
            new[]{CSharpSyntaxTree.ParseText(Resource("NetworkPerformance.cs.txt")),CSharpSyntaxTree.ParseText(Resource("NetworkPerformanceTests.cs.txt"))},
            ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(p=>MetadataReference.CreateFromFile(p)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var output=new MemoryStream();var result=compilation.Emit(output);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics));
        System.Reflection.Assembly.Load(output.ToArray()).GetType("NetworkPerformanceTestRunner")!.GetMethod("Run")!.Invoke(null,null);
        Console.WriteLine("PASS: snapshot acknowledgement, lost-response recovery, host/session isolation, old-server fallback and independent queued headers.");
    }
}
