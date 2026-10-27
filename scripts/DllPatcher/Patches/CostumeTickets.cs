using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Mono.Cecil;
using Mono.Cecil.Cil;
namespace DllPatcher;
partial class Program {
    const BindingFlags CTFlags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
    static object CTMember(object o,string name) {
        for(var t=o.GetType();t!=null;t=t.BaseType) {
            var p=t.GetProperty(name,CTFlags|BindingFlags.DeclaredOnly);if(p!=null)return p.GetValue(o)!;
            var f=t.GetField(name,CTFlags|BindingFlags.DeclaredOnly);if(f!=null)return f.GetValue(o)!;
        }
        throw new InvalidOperationException("Missing native member "+name);
    }
    static object CTCall(object o,string name,params object?[] args) => o.GetType().GetMethods(CTFlags)
        .Single(m=>m.Name==name && m.GetParameters().Length==args.Length && m.GetParameters().Select((p,i)=>args[i]==null || p.ParameterType.IsInstanceOfType(args[i])).All(x=>x)).Invoke(o,args)!;
    static string CTSnapshot(object o)=>JsonSerializer.Serialize(o.GetType().GetProperties().Where(p=>p.CanWrite).ToDictionary(p=>p.Name,p=>p.GetValue(o)));
    static string CTHash(byte[] b)=>Convert.ToHexString(SHA256.HashData(b)).ToLowerInvariant();
    static object CTKey(object row) => row.GetType().GetInterfaces().Single(t=>t.Name=="IDataKeySelector`1").GetProperty("Key")!.GetValue(row)!;
    static void CTIcon(ModuleDefinition game,JsonNode rules,bool verifyOnly) {
        var m=game.GetType("NGame2.NUI.UIHelper").Methods.Single(m=>m.Name=="GetItemIconPath" && m.Parameters.Count==1 && m.Parameters[0].ParameterType.FullName=="System.String");
        var code=rules["LightTicketCode"]!.GetValue<string>();
        var icon="Icon/Item/"+rules["IconSourceCode"]!.GetValue<string>();
        if(!m.Body.Instructions.Any(i=>Equals(i.Operand,code))) {
            if(verifyOnly)throw new InvalidOperationException("Light ticket icon alias missing");
            var il=m.Body.GetILProcessor();var first=m.Body.Instructions[0];
            var equal=new MethodReference("op_Equality",game.TypeSystem.Boolean,game.TypeSystem.String);
            equal.Parameters.Add(new ParameterDefinition(game.TypeSystem.String));equal.Parameters.Add(new ParameterDefinition(game.TypeSystem.String));
            foreach(var i in new[]{il.Create(OpCodes.Ldarg_0),il.Create(OpCodes.Ldstr,code),il.Create(OpCodes.Call,equal),il.Create(OpCodes.Brfalse,first),il.Create(OpCodes.Ldstr,icon),il.Create(OpCodes.Ret)})il.InsertBefore(first,i);
        }
        if(m.Body.Instructions.Count(i=>Equals(i.Operand,code))!=1 || !m.Body.Instructions.Any(i=>Equals(i.Operand,icon)))throw new InvalidOperationException("Invalid ticket icon alias");
        VerifyArenaResultStack(m);
    }
    static void InstallCostumeTickets(string clientRoot,bool stageOnly,bool verifyOnly) {
        clientRoot=Path.GetFullPath(clientRoot);
        var managed=Path.Combine(clientRoot,"King's Raid_Data","Managed");
        var patch=Path.Combine(clientRoot,"King's Raid_Data","Documents","Patch","StandaloneWindows");
        var staged=Path.GetFullPath(Path.Combine("target","costume-tickets",CTHash(System.Text.Encoding.UTF8.GetBytes(clientRoot))[..12]));
        using var input=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.CostumeTicketRules.json")!;
        var rules=JsonNode.Parse(input)!;
        var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"),new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        CTIcon(game.MainModule,rules,verifyOnly);
        AppDomain.CurrentDomain.AssemblyResolve+=(_,e)=>{var p=Path.Combine(managed,new AssemblyName(e.Name).Name+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;};
        var assembly=Assembly.Load(File.ReadAllBytes(Path.Combine(managed,"Assembly-CSharp.dll")));
        var outputs=new Dictionary<string,byte[]>();
        var originals=new Dictionary<string,byte[]>();
        foreach(var name in new[]{"SPRK-NativeData.json","SPRK-NativeLocalization.json","SPRK-NativeCombat.json"})originals[name]=File.ReadAllBytes(Path.Combine(patch,name));
        var metadata=originals.ToDictionary(x=>x.Key,x=>JsonNode.Parse(x.Value)!);
        void Save(string relative,byte[] bytes) {
            if(!originals.ContainsKey(relative))originals[relative]=File.ReadAllBytes(Path.Combine(patch,relative));
            foreach(var meta in metadata.Values)if(meta["files"]?[relative] is JsonNode entry) {
                if(entry["sha256"]!.GetValue<string>()!=CTHash(originals[relative]))throw new InvalidOperationException("Native integrity mismatch: "+relative);
                entry["sha256"]=CTHash(bytes);
            }
            var owner=metadata[relative.StartsWith("LocalizationJit/") && !relative.EndsWith("_English.jit")?"SPRK-NativeLocalization.json":"SPRK-NativeData.json"];
            if(owner["files"]?[relative]==null) owner["files"]![relative]=new JsonObject{["sha256"]=CTHash(bytes),["originalSha256"]=CTHash(originals[relative])};
            outputs[relative]=bytes;
        }
        byte[] Bake(string relative,object packer,byte[] original,int skip=0) {
            using var reader=new BinaryReader(new MemoryStream(original));reader.BaseStream.Position=skip;
            var code=reader.ReadBytes(reader.ReadInt32());var table=reader.ReadBytes(reader.ReadInt32());
            return (byte[])CTCall(packer,"SerializeBytes",Path.GetFileName(relative),table,code,0);
        }
        Dictionary<string,object> ReadRows(string name,out object container,out byte[] bytes) {
            var relative="TableJit/"+name+"Table.jit";bytes=File.ReadAllBytes(Path.Combine(patch,relative));
            var type=assembly.GetType("NShared."+(name=="Item"?"ItemBase":name)+"DataContainer")!;
            container=Activator.CreateInstance(type)!;CTCall(CTMember(container,"Packer"),"Deserialize",bytes);
            return ((IEnumerable)CTCall(container,"GetAllData")).Cast<object>().ToDictionary(o=>CTKey(o).ToString()!);
        }
        var costumes=ReadRows("Costume",out var costumeContainer,out var costumeBytes);
        foreach(var update in rules["CostumeUpdates"]!.AsArray()) {
            var row=costumes[update!["CostumeIndex"]!.ToString()];
            foreach(var pair in update.AsObject().Where(p=>p.Key!="CostumeIndex")) {
                var property=row.GetType().GetProperty(pair.Key)!;
                property.SetValue(row,JsonSerializer.Deserialize(pair.Value!.ToJsonString(),property.PropertyType));
            }
        }
        foreach(var json in rules["NewCostumes"]!.AsArray()) {
            var key=json!["CostumeIndex"]!.ToString();var row=JsonSerializer.Deserialize(json.ToJsonString(),assembly.GetType("NShared.CostumeData")!)!;
            if(costumes.TryGetValue(key,out var existing) && CTSnapshot(existing)!=CTSnapshot(row))throw new InvalidOperationException("Light costume index collision: "+key);
            costumes[key]=row;
        }
        var roster=rules["HeroIndices"]!.AsArray().Select(x=>x!.GetValue<int>()).Order().ToArray();
        foreach(var key in new[]{"DarkCostumeIndices","LightCostumeIndices"}) {
            var ids=rules[key]!.AsArray().Select(x=>x!.GetValue<int>()).ToArray();
            var heroes=ids.Select(i=>(int)CTMember(costumes[i.ToString()],"HeroIndex")).Order().ToArray();
            if(ids.Length!=102 || ids.Distinct().Count()!=102 || !heroes.SequenceEqual(roster))throw new InvalidOperationException("Invalid 102-hero native pool: "+key);
        }
        void WriteRows(string name,object old,byte[] original,Dictionary<string,object> rows) {
            var relative="TableJit/"+name+"Table.jit";
            var originalContainer=Activator.CreateInstance(old.GetType())!;CTCall(CTMember(originalContainer,"Packer"),"Deserialize",original);
            var before=((IEnumerable)CTCall(originalContainer,"GetAllData")).Cast<object>().ToDictionary(o=>CTKey(o).ToString()!,CTSnapshot);
            var expected=rows.ToDictionary(x=>x.Key,x=>CTSnapshot(x.Value));
            bool changed=before.Count!=expected.Count || before.Any(x=>!expected.TryGetValue(x.Key,out var s)||s!=x.Value);
            if(verifyOnly && changed)throw new InvalidOperationException("Native ticket table needs update: "+name);
            byte[] baked=original;
            if(changed) {
                var fresh=Activator.CreateInstance(old.GetType())!;
                var list=(IList)CTCall(CTMember(fresh,"BaseList"),"JsonLoad_GetOrNew");
                var dic=(IDictionary)CTCall(CTMember(fresh,"BaseDic"),"JsonLoad_Get");
                foreach(var row in rows.Values){list.Add(row);dic.Add(CTKey(row),row);}
                CTCall(CTMember(fresh,"_firstData"),"JsonLoad_Set",rows.Values.First());
                CTCall(fresh,"OnEndLoadFromJsonData");baked=Bake(relative,CTMember(fresh,"Packer"),original);
            }
            var reread=Activator.CreateInstance(old.GetType())!;CTCall(CTMember(reread,"Packer"),"Deserialize",baked);
            var actual=((IEnumerable)CTCall(reread,"GetAllData")).Cast<object>().ToDictionary(o=>CTKey(o).ToString()!,CTSnapshot);
            if(actual.Count!=expected.Count || expected.Any(x=>!actual.TryGetValue(x.Key,out var v)||v!=x.Value))throw new InvalidOperationException("Native table readback mismatch: "+name+" "+string.Join(";",expected.Where(x=>!actual.TryGetValue(x.Key,out var v)||v!=x.Value).Take(1).Select(x=>x.Key+" EXPECTED "+x.Value+" ACTUAL "+actual.GetValueOrDefault(x.Key))));
            foreach(var row in rows.Values)if(CTSnapshot(CTCall(reread,"GetData",CTKey(row)))!=expected[CTKey(row).ToString()!])throw new InvalidOperationException("Native dictionary lookup mismatch: "+name);
            if(name=="Item")foreach(var row in rows.Values.Where(r=>CTKey(r).ToString()==rules["LightTicketItemIndex"]!.ToString()))if(CTSnapshot(CTCall(reread,"GetDataByCode",CTMember(row,"Code")))!=expected[CTKey(row).ToString()!])throw new InvalidOperationException("Native item code lookup mismatch");
            Save(relative,baked);
        }
        // Resolve the exact native model and UI entries used by every choice.
        var bundlePath=Path.Combine(patch,"Assetbundle","BundlePack.bin");
        if(File.Exists(bundlePath) && CTHash(File.ReadAllBytes(bundlePath))!=rules["AssetEvidence"]!["BundlePackSha256"]!.GetValue<string>())
            throw new InvalidOperationException("BundlePack changed; re-audit costume asset evidence.");
        // Release baselines contain managed update files, without the original model pack.
        // Evidence was extracted from and checked against the installed pack's exact hash.
        var uiKeys=rules["AssetEvidence"]!["PortraitPaths"]!.AsArray().Select(x=>x!.GetValue<string>()).ToHashSet();
        if(!uiKeys.Contains("Icon/Item/"+rules["IconSourceCode"]!.GetValue<string>()))throw new InvalidOperationException("Missing Light ticket icon source");
        foreach(var id in rules["DarkCostumeIndices"]!.AsArray().Concat(rules["LightCostumeIndices"]!.AsArray()).Select(x=>x!.GetValue<int>()).Distinct()) {
            var costume=costumes[id.ToString()];var hero=CTMember(costume,"HeroIndex").ToString()!;
            var prefix=(string)CTMember(costume,"BodyPrefix");
            var models=rules["AssetEvidence"]!["Models"]![hero]!.AsArray();
            if(!models.Any(model=>model!["Prefix"]!.GetValue<string>()==prefix && model["AssetName"]!.GetValue<string>()!=""))throw new InvalidOperationException("Missing native costume model: "+id);
            foreach(var property in new[]{"PortraitPath","IconPortraitPath","BattlePortraitPath"}) {
                var path=(string)CTMember(costume,property);
                if(!uiKeys.Contains(path))throw new InvalidOperationException("Missing costume portrait: "+id+" "+path);
            }
        }
        WriteRows("Costume",costumeContainer,costumeBytes,costumes);
        var items=ReadRows("Item",out var itemContainer,out var itemBytes);
        var itemJson=rules["NewItem"]!;var itemRow=JsonSerializer.Deserialize(itemJson.ToJsonString(),assembly.GetType("NShared.ItemBaseData")!)!;
        var itemKey=rules["LightTicketItemIndex"]!.ToString();
        if(items.TryGetValue(itemKey,out var currentItem)&&CTSnapshot(currentItem)!=CTSnapshot(itemRow))throw new InvalidOperationException("New ticket item index collision");
        if(items.Values.Any(x=>(string)CTMember(x,"Code")==rules["LightTicketCode"]!.GetValue<string>() && CTKey(x).ToString()!=itemKey))throw new InvalidOperationException("New ticket code collision");
        items[itemKey]=itemRow;WriteRows("Item",itemContainer,itemBytes,items);
        var selectors=ReadRows("CostumeSelectItem",out var selectorContainer,out var selectorBytes);
        var dark=selectors[rules["DarkTicketCode"]!.GetValue<string>()];dark.GetType().GetProperty("CostumeIndices")!.SetValue(dark,rules["DarkCostumeIndices"]!.AsArray().Select(x=>x!.GetValue<int>()).ToArray());
        var selectorJson=rules["NewSelector"]!;var selectorRow=JsonSerializer.Deserialize(selectorJson.ToJsonString(),assembly.GetType("NShared.CostumeSelectItemData")!)!;
        var selectorKey=rules["LightTicketCode"]!.GetValue<string>();
        if(selectors.TryGetValue(selectorKey,out var currentSelector)&&CTSnapshot(currentSelector)!=CTSnapshot(selectorRow))throw new InvalidOperationException("New ticket selector collision");
        selectors[selectorKey]=selectorRow;WriteRows("CostumeSelectItem",selectorContainer,selectorBytes,selectors);
        // Local additions use English fallback text in every shipped language.
        var build=assembly.GetType("NClientData.NLocalization.BuildTools_for_Localize")!;
        var hash=assembly.GetType("NShared.NJit.JitOption")!.GetMethod("StringToHashCode")!;
        foreach(var path in Directory.GetFiles(Path.Combine(patch,"LocalizationJit"),"LocalizationAll_*.jit")) {
            var original=File.ReadAllBytes(path);var old=build.GetMethod("Deserialize")!.Invoke(null,new object[]{original})!;
            var dictionary=CTMember(old,"_dic");CTCall(dictionary,"DevOnly_ForceLoadAllRawData");
            var cache=(IDictionary)CTCall(dictionary,"JsonLoad_Get");
            var changes=new Dictionary<object,string>();
            foreach(var pair in rules["Localization"]!.AsObject()) {
                object key=original[0]==1?hash.Invoke(null,new object[]{pair.Key})!:pair.Key;
                var value=pair.Value!.GetValue<string>();if(!cache.Contains(key)||!Equals(cache[key],value))changes.Add(key,value);
            }
            if(verifyOnly && changes.Count!=0)throw new InvalidOperationException("Ticket localization missing: "+path);
            byte[] baked=original;
            if(changes.Count>0) {
                var fresh=Activator.CreateInstance(old.GetType())!;var target=(IDictionary)CTCall(CTMember(fresh,"_dic"),"JsonLoad_Get");
                foreach(DictionaryEntry pair in cache)target.Add(pair.Key,pair.Value);
                foreach(var pair in changes)target[pair.Key]=pair.Value;
                var packed=Bake(Path.GetFileName(path),CTMember(fresh,"_packer"),original,1);
                baked=new[]{original[0]}.Concat(packed).ToArray();
                var check=build.GetMethod("Deserialize")!.Invoke(null,new object[]{baked})!;var checkDic=CTMember(check,"_dic");CTCall(checkDic,"DevOnly_ForceLoadAllRawData");
                var actual=(IDictionary)CTCall(checkDic,"JsonLoad_Get");
                if(actual.Count!=target.Count)throw new InvalidOperationException("Localization count mismatch");
                foreach(DictionaryEntry pair in target)if(!actual.Contains(pair.Key)||!Equals(actual[pair.Key],pair.Value))throw new InvalidOperationException("Localization readback mismatch");
            }
            Save("LocalizationJit/"+Path.GetFileName(path),baked);
        }
        foreach(var pair in metadata)outputs[pair.Key]=JsonSerializer.SerializeToUtf8Bytes(pair.Value,new JsonSerializerOptions{WriteIndented=true});
        Console.WriteLine("Verified 102 Darkness choices, 102 Light choices, 73 new Light rows, complete native table lookups and preserved localization entries.");
        if(verifyOnly)return;
        Directory.CreateDirectory(staged);
        foreach(var pair in outputs){var dest=Path.Combine(staged,pair.Key);Directory.CreateDirectory(Path.GetDirectoryName(dest)!);File.WriteAllBytes(dest,pair.Value);}
        game.Write(Path.Combine(staged,"Assembly-CSharp.dll"));
        if(stageOnly){Console.WriteLine("Staged costume tickets: "+staged);return;}
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,clientRoot)))throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
        foreach(var pair in originals)if(!File.ReadAllBytes(Path.Combine(patch,pair.Key)).SequenceEqual(pair.Value))throw new InvalidOperationException("Client changed during ticket staging");
        var backup=Path.Combine(staged,"backups",DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff"));Directory.CreateDirectory(backup);
        File.Copy(Path.Combine(managed,"Assembly-CSharp.dll"),Path.Combine(backup,"Assembly-CSharp.dll"));
        foreach(var pair in outputs) {
            var dest=Path.Combine(patch,pair.Key);var saved=Path.Combine(backup,pair.Key);Directory.CreateDirectory(Path.GetDirectoryName(saved)!);File.Copy(dest,saved);
            File.Copy(Path.Combine(staged,pair.Key),dest,true);
        }
        File.Move(Path.Combine(staged,"Assembly-CSharp.dll"),Path.Combine(managed,"Assembly-CSharp.dll"),true);
        if(!NativeCombatTables.ValidateInstalled(managed))throw new InvalidOperationException("Native combat metadata missing");
        if(!NativeStaticData.ValidateInstalled(managed))throw new InvalidOperationException("Native data metadata missing");
        Console.WriteLine("Installed all-roster costume tickets and verified native metadata. Restart the client.");
    }
}


