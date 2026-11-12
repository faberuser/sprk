using System.Reflection;
using System.Text.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace DllPatcher;
partial class Program {
    static void PatchHeroDeathVoices(ModuleDefinition module) {
        var stock=module.GetType("NGame2.NBattleView.Creature").Methods.Single(m=>m.Name=="SetSoundStock");
        foreach(var i in stock.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ldstr))
            i.Operand=i.Operand switch {"DEAD"=>"Dead","DAMAGE"=>"Damage","DOWN"=>"Down",var value=>value};
        var helper=module.GetType("NGameAsset.NSound.SoundHelper");
        // Only the overloads that receive a separate prefix/body need a dot.
        // The two-argument overload receives an already complete sound code.
        foreach(var method in helper.Methods.Where(m=>(m.Name=="GetSoundCode" && m.Parameters.Count==3) || (m.Name=="GetSoundInstance" && m.Parameters.Count==4)))
            foreach(var i in method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ldstr && Equals(i.Operand,"{0}{1}")))i.Operand="{0}.{1}";
        VerifyHeroDeathVoices(module);
    }

    static void VerifyHeroDeathVoices(ModuleDefinition module) {
        var creature=module.GetType("NGame2.NBattleView.Creature");
        var stock=creature.Methods.Single(m=>m.Name=="SetSoundStock");
        var prefixes=stock.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ldstr).Select(i=>(string)i.Operand).ToArray();
        if(!prefixes.SequenceEqual(new[]{"Dead","Damage","Down"}))throw new InvalidOperationException("Creature voice prefixes must match sound-table casing.");
        var helper=module.GetType("NGameAsset.NSound.SoundHelper");
        foreach(var method in helper.Methods.Where(m=>(m.Name=="GetSoundCode" && m.Parameters.Count==3) || (m.Name=="GetSoundInstance" && m.Parameters.Count==4))) {
            var formats=method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ldstr).Select(i=>i.Operand).ToArray();
            if(!formats.SequenceEqual(new[]{"{0}.{1}{2}","{0}.{1}"}))throw new InvalidOperationException("Costume voice fallback must preserve prefix/body separator.");
            VerifyArenaResultStack(method);
        }
        var damage=creature.Methods.Single(m=>m.Name=="OnMessageFast" && m.Parameters.Single().ParameterType.FullName.Contains("NShared.Damage"));
        foreach(var name in new[]{"get_IsDead","Die","TryPlayOnce","EnableAnimationSound"})
            if(!damage.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name==name))throw new InvalidOperationException("Native death voice path missing: "+name);
        VerifyArenaResultStack(stock);
        Console.WriteLine("Verified case-correct death/damage/down codes, costume fallback separator and retained native death playback path.");
    }

    static void InstallHeroDeathVoices(string root,bool stageOnly,bool verifyOnly=false) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));var dll=Path.Combine(managed,"Assembly-CSharp.dll");
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        if(verifyOnly)VerifyHeroDeathVoices(game.MainModule);else PatchHeroDeathVoices(game.MainModule);
        var size=game.MainModule.GetType("NGameAsset.NSound.SoundHelper").Methods.Where(m=>m.HasBody).Sum(m=>m.Body.Instructions.Count);
        PatchHeroDeathVoices(game.MainModule);
        if(size!=game.MainModule.GetType("NGameAsset.NSound.SoundHelper").Methods.Where(m=>m.HasBody).Sum(m=>m.Body.Instructions.Count))throw new Exception("Voice patch is not idempotent.");
        TestHeroDeathVoices(game,managed,root);
        if(verifyOnly)return;
        var staged=dll+".hero-death-voices-staged";game.Write(staged);
        using(var check=AssemblyDefinition.ReadAssembly(staged))VerifyHeroDeathVoices(check.MainModule);
        if(stageOnly)return;
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))throw new InvalidOperationException("Voice fix staged. Close game clients before installation.");
        var backup=Path.Combine("target","hero-death-voices",DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));Directory.CreateDirectory(backup);
        File.Copy(dll,Path.Combine(backup,"Assembly-CSharp.dll"));File.Move(staged,dll,true);
        Console.WriteLine("Installed hero death voice lookup fix. Restart the game.");
    }

    static void TestHeroDeathVoices(AssemblyDefinition game,string managed,string root) {
        // Execute the actual patched code builder with the installed catalogue.
        // Replace only its availability query; loading/decoding localized sound
        // rows below still uses the game's native SoundDataContainer.
        // The CI release baseline carries only launcher-managed files, not the
        // stock BundlePack.bin; static IL verification still runs there.
        var bundlePack=Path.Combine(root,"King's Raid_Data","Documents","Patch","StandaloneWindows","Assetbundle","BundlePack.bin");
        if(!File.Exists(bundlePack)){Console.WriteLine("Skipped hero death voice catalogue test: BundlePack.bin not present.");return;}
        using var bytes=new MemoryStream();game.Write(bytes);bytes.Position=0;
        using var fixture=AssemblyDefinition.ReadAssembly(bytes,new ReaderParameters{AssemblyResolver=game.MainModule.AssemblyResolver});
        var module=fixture.MainModule;var helper=module.GetType("NGameAsset.NSound.SoundHelper");
        var catalog=new FieldDefinition("SPRKFixtureCodes",Mono.Cecil.FieldAttributes.Public|Mono.Cecil.FieldAttributes.Static,module.TypeSystem.String);helper.Fields.Add(catalog);
        var exists=helper.Methods.Single(m=>m.Name=="IsExistSoundData");exists.Body=new MethodBody(exists);
        var concat=new MethodReference("Concat",module.TypeSystem.String,module.TypeSystem.String);
        for(int i=0;i<3;i++)concat.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));
        var contains=new MethodReference("Contains",module.TypeSystem.Boolean,module.TypeSystem.String){HasThis=true};contains.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));
        var il=exists.Body.GetILProcessor();il.Emit(OpCodes.Ldsfld,catalog);il.Emit(OpCodes.Ldstr,"|");il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldstr,"|");il.Emit(OpCodes.Call,concat);il.Emit(OpCodes.Callvirt,contains);il.Emit(OpCodes.Ret);
        using var output=new MemoryStream();fixture.Write(output);
        ResolveEventHandler resolve=(_,e)=>{var path=Path.Combine(managed,new AssemblyName(e.Name).Name+".dll");return File.Exists(path)?Assembly.LoadFrom(path):null;};
        AppDomain.CurrentDomain.AssemblyResolve+=resolve;
        try {
            var assembly=Assembly.Load(output.ToArray());
            var read=assembly.GetType("JsonBin.JsonRead")!;
            var create=read.GetMethods().Single(m=>m.Name=="CreateRoot" && m.GetParameters().Length==2 && m.GetParameters()[0].ParameterType==typeof(byte[]));
            object?[] parameters={File.ReadAllBytes(bundlePack),null};
            if(!(bool)create.Invoke(null,parameters)!)throw new Exception("Installed sound pack cannot be decoded.");
            var soundRoot=read.GetMethod("GetField",new[]{typeof(string)})!.Invoke(parameters[1],new object[]{"SoundTable_Json"})!;
            var decode=assembly.GetType("JsonBin.NBinary.BinaryToJson")!.GetMethods().Single(m=>m.Name=="ToJson" && m.GetParameters()[0].ParameterType==read);
            var nativeJson=decode.Invoke(null,new[]{soundRoot})!;
            var text=(string)nativeJson.GetType().GetMethod("internalToJson",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(nativeJson,new object[]{1024,false})!;
            using var json=JsonDocument.Parse(text);var rows=json.RootElement.EnumerateObject().ToArray();var codes=rows.Select(r=>r.Name).ToHashSet(StringComparer.Ordinal);
            var type=assembly.GetType(helper.FullName)!;type.GetField(catalog.Name)!.SetValue(null,"|"+string.Join("|",codes)+"|");
            var get=type.GetMethods().Single(m=>m.Name=="GetSoundCode" && m.GetParameters().Length==3);
            string Build(string prefix,string hero,string postfix)=>(string)get.Invoke(null,new object[]{prefix,hero,postfix})!;
            int cases=0;
            var deaths=rows.Where(r=>r.Name.StartsWith("Dead.Hero.",StringComparison.Ordinal) && r.Value.TryGetProperty("CreatureCode",out _)).ToArray();
            foreach(var row in deaths) {
                string hero=row.Value.GetProperty("CreatureCode").GetString()!;var baseCode="Dead."+hero;
                if(!row.Name.StartsWith(baseCode,StringComparison.Ordinal))throw new Exception("Unexpected hero death code: "+row.Name);
                string postfix=row.Name[baseCode.Length..];
                if(Build("Dead",hero,postfix)!=row.Name)throw new Exception("Existing costume death voice lost: "+row.Name);
                if(codes.Contains(baseCode) && Build("Dead",hero,"_SPRKMissingVoice")!=baseCode)throw new Exception("Missing costume voice did not fall back: "+hero);
                cases+=2;
            }
            if(deaths.Length<100 || codes.Contains(Build("DEAD","Hero.Frey","")))throw new Exception("Death-voice negative control failed.");
            var container=Activator.CreateInstance(assembly.GetType("NGameAsset.NSound.SoundDataContainer")!)!;
            container.GetType().GetMethod("LoadFromJson")!.Invoke(container,new[]{soundRoot});
            var language=assembly.GetType("UnityEngine.SystemLanguage") ?? assembly.GetReferencedAssemblies().Select(n=>Assembly.Load(n).GetType("UnityEngine.SystemLanguage")).First(t=>t!=null)!;
            var lookup=container.GetType().GetMethods().Single(m=>m.Name=="GetSound" && m.GetParameters().Length==2);
            int localized=0;
            foreach(var row in deaths)foreach(var name in new[]{"English","Korean","Japanese"}) {
                var sound=lookup.Invoke(container,new[]{Enum.Parse(language,name),(object)row.Name});
                if(sound==null)throw new Exception("Native localized death sound not found: "+name+"/"+row.Name);
                var clips=(System.Collections.IList)sound.GetType().GetField("ClipList")!.GetValue(sound)!;
                if(clips.Count==0)throw new Exception("Death sound has no clips: "+name+"/"+row.Name);
                localized++;
            }
            Console.WriteLine($"PASS: {deaths.Length} hero/costume death rows, {cases} native lookup/fallback cases, {localized} native English/Korean/Japanese sound loads and original-uppercase negative control.");
        } finally {AppDomain.CurrentDomain.AssemblyResolve-=resolve;}
    }
}
