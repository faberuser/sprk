using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DllPatcher;
partial class Program {
    internal static bool IsLoginProviderAssembly(string name) =>
        name is "com.rlabrecque.steamworks.net" or "Google.MiniJson" or
            "Facebook.Unity" or "Facebook.Unity.Settings" or "Facebook.Unity.Gameroom" or "FacebookNamedPipeClient";

    static bool ProviderType(TypeReference type) =>
        type.Scope is AssemblyNameReference scope && IsLoginProviderAssembly(scope.Name) ||
        type is TypeSpecification spec && ProviderType(spec.ElementType) ||
        type is GenericInstanceType generic && generic.GenericArguments.Any(ProviderType);

    static void RemoveLoginProviderReferences(ModuleDefinition module) {
        // Retain the game's IAP interface so shop code can report billing unavailable.
        // The external Steam SDK and its startup component are completely removed.
        var billing=module.GetType("NGame2.IAP.Steam.SteamIAPManager");
        if(billing!=null) {
            var init=billing.Methods.Single(m=>m.Name=="Init");
            var callback=init.Parameters[0].ParameterType;
            var invoke=init.Body.Instructions.Select(i=>i.Operand).OfType<MethodReference>()
                .First(m=>m.Name=="Invoke" && m.DeclaringType.FullName==callback.FullName);
            init.Body=new MethodBody(init);var il=init.Body.GetILProcessor();
            var done=Instruction.Create(OpCodes.Ret);
            // Use the existing enum value rather than assuming its numeric representation.
            int unavailable=Convert.ToInt32(module.GetType("NGame2.IAP.EResultInit").Fields.Single(f=>f.Name=="BillingNotSupported").Constant);
            il.Append(Instruction.Create(OpCodes.Ldarg_1));il.Append(Instruction.Create(OpCodes.Brfalse,done));
            il.Append(Instruction.Create(OpCodes.Ldarg_1));il.Append(Instruction.Create(OpCodes.Ldc_I4,unavailable));
            il.Append(Instruction.Create(OpCodes.Ldstr,"External billing is unavailable."));
            il.Append(Instruction.Create(OpCodes.Callvirt,invoke));il.Append(done);
            var id=billing.Methods.Single(m=>m.Name=="GetSteamID");
            id.Body=new MethodBody(id);id.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
            id.Body.Instructions.Add(Instruction.Create(OpCodes.Conv_U8));id.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            foreach(var method in billing.Methods.Where(m=>m.Parameters.Any(p=>ProviderType(p.ParameterType))).ToArray())
                billing.Methods.Remove(method);
            foreach(var field in billing.Fields.Where(f=>ProviderType(f.FieldType)).ToArray())billing.Fields.Remove(field);
        }
        var manager=module.GetType("NGame2.NAccount.MasangLoginManager");
        if(manager!=null) {
            var invoke=manager.NestedTypes.Single(t=>t.Name=="LinkedCallback").Methods.Single(m=>m.Name=="Invoke");
            int failed=Convert.ToInt32(manager.NestedTypes.Single(t=>t.Name=="LoginSuccessState").Fields.Single(f=>f.Name=="Failed").Constant);
            foreach(var provider in new[]{"Steam","Google","Apple"}) {
                var linked=manager.Methods.Single(m=>m.Name=="Linked"+provider);
                linked.Body=new MethodBody(linked);var il=linked.Body.GetILProcessor();var done=Instruction.Create(OpCodes.Ret);
                il.Append(Instruction.Create(OpCodes.Ldarg_1));il.Append(Instruction.Create(OpCodes.Brfalse,done));
                il.Append(Instruction.Create(OpCodes.Ldarg_1));il.Append(Instruction.Create(OpCodes.Ldc_I4,failed));
                il.Append(Instruction.Create(OpCodes.Ldstr,"Sign in with your SPRK username and password."));
                il.Append(Instruction.Create(OpCodes.Callvirt,invoke));il.Append(done);
                foreach(var name in new[]{"Start"+provider+"Login","StartLinked"+provider}) {
                    foreach(var method in manager.Methods.Where(m=>m.Name==name).ToArray())manager.Methods.Remove(method);
                    foreach(var nested in manager.NestedTypes.Where(t=>t.Name.StartsWith("<"+name+">",StringComparison.Ordinal)).ToArray())manager.NestedTypes.Remove(nested);
                }
            }
        }
        foreach(var method in module.GetTypes().Where(t=>t.FullName is not ("SteamManager" or "NGame2.Steam.SteamScript"))
            .SelectMany(t=>t.Methods).Where(m=>m.HasBody))
            foreach(var instruction in method.Body.Instructions)
                if(instruction.Operand is MethodReference call && call.DeclaringType.FullName=="SteamManager" && call.Name=="get_Initialized") {
                    instruction.OpCode=OpCodes.Ldc_I4_0;instruction.Operand=null;
                }
        foreach(var type in module.Types.Where(t=>t.FullName is "SteamManager" or "NGame2.Steam.SteamScript").ToArray())module.Types.Remove(type);
        foreach(var reference in module.AssemblyReferences.Where(r=>IsLoginProviderAssembly(r.Name)).ToArray())module.AssemblyReferences.Remove(reference);
        // Cecil's input TypeRef table includes unused rows until the module is written.
        using var bytes=new MemoryStream();module.Write(bytes);bytes.Position=0;
        using var rewritten=ModuleDefinition.ReadModule(bytes);
        var remaining=rewritten.GetTypeReferences().Where(ProviderType).Select(t=>t.FullName).ToArray();
        if(remaining.Length!=0)throw new InvalidOperationException("Provider references remain: "+string.Join(", ",remaining));
    }

    static void StageLoginProviderRemoval(string clientRoot,string output) {
        var data=Path.GetFullPath(Path.Combine(clientRoot,"King's Raid_Data"));
        var stage=Path.GetFullPath(output);
        if(stage.StartsWith(data+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new ArgumentException("Stage outside the client directory.");
        Directory.CreateDirectory(Path.Combine(stage,"Managed"));
        var removedFiles=new List<string>();var modifiedFiles=new List<string>();
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(Path.Combine(data,"Managed"));
        var removedTypes=new[]{"Assembly-CSharp:SteamManager","Assembly-CSharp:NGame2.Steam.SteamScript"};
        foreach(var path in Directory.GetFiles(Path.Combine(data,"Managed"),"*.dll")) {
            using var assembly=AssemblyDefinition.ReadAssembly(path,new ReaderParameters{AssemblyResolver=resolver});
            if(IsLoginProviderAssembly(assembly.Name.Name)) {
                removedFiles.Add(Path.GetRelativePath(data,path));
                foreach(var suffix in new[]{".pdb",".dll.mdb"}) {
                    var symbols=suffix==".pdb"?Path.ChangeExtension(path,suffix):Path.ChangeExtension(path,null)+suffix;
                    if(File.Exists(symbols))removedFiles.Add(Path.GetRelativePath(data,symbols));
                }
            } else {
                if(assembly.Name.Name=="Assembly-CSharp") {
                    if(!assembly.MainModule.AssemblyReferences.Any(r=>r.Name=="SprkAccounts"))
                        throw new InvalidOperationException("Install --accounts-only before removing provider SDKs.");
                    RemoveLoginProviderReferences(assembly.MainModule);
                    assembly.Write(Path.Combine(stage,"Managed","Assembly-CSharp.dll"));
                    modifiedFiles.Add(Path.GetRelativePath(data,path));
                } else if(assembly.MainModule.AssemblyReferences.Any(r=>IsLoginProviderAssembly(r.Name)))
                    throw new InvalidOperationException("Retained assembly depends on a provider: "+assembly.Name.Name);
            }
        }
        foreach(var path in Directory.GetFiles(Path.Combine(data,"Plugins"),"*",SearchOption.AllDirectories))
            if(Path.GetFileName(path) is "steam_api.dll" or "steam_api64.dll" or "Steamworks.NET.txt")removedFiles.Add(Path.GetRelativePath(data,path));
        foreach(var name in new[]{"RuntimeInitializeOnLoads.json","ScriptingAssemblies.json"}) {
            var json=JsonNode.Parse(File.ReadAllText(Path.Combine(data,name)))!;
            if(name=="RuntimeInitializeOnLoads.json") {
                var hooks=json["root"]!.AsArray();
                foreach(var hook in hooks.ToArray())
                    if(IsLoginProviderAssembly(hook!["assemblyName"]!.GetValue<string>()) ||
                        hook["assemblyName"]!.GetValue<string>()=="Assembly-CSharp" && hook["className"]!.GetValue<string>()=="SteamManager")hooks.Remove(hook);
            } else {
                var names=json["names"]!.AsArray();var types=json["types"]!.AsArray();
                for(int i=names.Count-1;i>=0;i--)
                    if(IsLoginProviderAssembly(Path.GetFileNameWithoutExtension(names[i]!.GetValue<string>()))) {names.RemoveAt(i);types.RemoveAt(i);}
            }
            File.WriteAllText(Path.Combine(stage,name),json.ToJsonString());modifiedFiles.Add(name);
        }
        File.WriteAllText(Path.Combine(stage,"manifest.json"),JsonSerializer.Serialize(new {removedFiles,modifiedFiles,removedTypes},new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"Staged {removedFiles.Count} provider file removals at {stage}");
    }
}
