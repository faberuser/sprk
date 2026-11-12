using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
namespace DllPatcher;
partial class Program {
    static void InstallCouponItems(string clientRoot, bool verifyOnly) {
        clientRoot=Path.GetFullPath(clientRoot);
        var managed=Path.Combine(clientRoot,"King's Raid_Data","Managed");
        var patch=Path.Combine(clientRoot,"King's Raid_Data","Documents","Patch","StandaloneWindows");
        if(!NativeCombatTables.ValidateInstalled(managed) || !NativeStaticData.ValidateInstalled(managed))throw new InvalidOperationException("Invalid native integrity metadata before coupon item installation");
        using var input=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.CouponPool.json")!;
        var rules=JsonNode.Parse(input)!;
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
        void WriteRows(string name,object old,byte[] original,Dictionary<string,object> rows) {
            var relative="TableJit/"+name+"Table.jit";
            var originalContainer=Activator.CreateInstance(old.GetType())!;CTCall(CTMember(originalContainer,"Packer"),"Deserialize",original);
            var before=((IEnumerable)CTCall(originalContainer,"GetAllData")).Cast<object>().ToDictionary(o=>CTKey(o).ToString()!,CTSnapshot);
            var expected=rows.ToDictionary(x=>x.Key,x=>CTSnapshot(x.Value));
            bool changed=before.Count!=expected.Count || before.Any(x=>!expected.TryGetValue(x.Key,out var s)||s!=x.Value);
            if(verifyOnly && changed)throw new InvalidOperationException("Native coupon item table needs update: "+name);
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
            if(name=="Item")foreach(var row in rows.Values.Where(r=>CTKey(r).ToString()=="9900001"))if(CTSnapshot(CTCall(reread,"GetDataByCode",CTMember(row,"Code")))!=expected[CTKey(row).ToString()!])throw new InvalidOperationException("Native item code lookup mismatch");
            Save(relative,baked);
        }
        var items=ReadRows("Item",out var itemContainer,out var itemBytes);
        foreach(var json in rules["Items"]!.AsArray()) {
            var row=JsonSerializer.Deserialize(json!.ToJsonString(),assembly.GetType("NShared.ItemBaseData")!)!;
            var key=CTKey(row).ToString()!;
            if(items.TryGetValue(key,out var current)&&CTSnapshot(current)!=CTSnapshot(row))throw new InvalidOperationException("Coupon item index collision: "+key);
            if(items.Values.Any(x=>Equals(CTMember(x,"Code"),CTMember(row,"Code")) && CTKey(x).ToString()!=key))throw new InvalidOperationException("Coupon item code collision");
            items[key]=row;
        }
        WriteRows("Item",itemContainer,itemBytes,items);
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
            if(verifyOnly && changes.Count!=0)throw new InvalidOperationException("Coupon item localization missing: "+path);
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
        if(verifyOnly) { Console.WriteLine("PASS: coupon item tables, index/code lookups, all shipped localization and integrity hashes."); return; }
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,clientRoot)))throw new InvalidOperationException("Close the game before installing coupon items.");
        var backup=Path.GetFullPath(Path.Combine("target","coupon-item-backups",DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff")));
        foreach(var pair in originals)if(!File.ReadAllBytes(Path.Combine(patch,pair.Key)).SequenceEqual(pair.Value))throw new InvalidOperationException("Client changed during coupon item staging");
        foreach(var pair in outputs) {
            var dest=Path.Combine(patch,pair.Key);var saved=Path.Combine(backup,pair.Key);Directory.CreateDirectory(Path.GetDirectoryName(saved)!);File.Copy(dest,saved);
            File.WriteAllBytes(dest,pair.Value);
        }
        if(!NativeCombatTables.ValidateInstalled(managed) || !NativeStaticData.ValidateInstalled(managed))throw new InvalidOperationException("Missing native integrity metadata");
        Console.WriteLine("Installed xdd coupon item with preserved native tables and localization. Backup: "+backup);
    }
}
