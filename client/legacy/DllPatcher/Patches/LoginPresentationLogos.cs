using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace DllPatcher;
partial class Program {
    static readonly Regex CenterLoginLogoName=new(@"^(Title_(Text\d+|Light|Effect(?:_Light)?)|UI_Title_Logo003_(Text\d+|Glow_(Once|Loop))|UI_Title_Logo005_Logo\d+|UI_Title_Logo005_Particle\d+\(Logo\)|Effect_UI_Title_Logo006_BI_Text)$",RegexOptions.CultureInvariant);
    static bool SoulWeaponLoginSprite(JsonNode row)=>row["type"]?.GetValue<string>()=="UISprite" && (row["data"]?["mSpriteName"]?.GetValue<string>()??"").StartsWith("Logo_SoulWeapon_",StringComparison.Ordinal);
    static string LoginContentDirectory(string root)=>Path.Combine(root,"King's Raid_Data/Documents/Patch/StandaloneWindows/LoginPresentation");
    static IEnumerable<string> LoginReferences(JsonNode? node) {
        if(node is JsonObject obj) {
            if(obj["ref"] is JsonValue value && value.TryGetValue<string>(out var id))yield return id;
            foreach(var pair in obj)foreach(var nested in LoginReferences(pair.Value))yield return nested;
        } else if(node is JsonArray array)foreach(var child in array)foreach(var id in LoginReferences(child))yield return id;
    }
    static bool LogoPath(string? path)=>path!=null && path.Split('/').Any(part=>CenterLoginLogoName.IsMatch(part));
    static void CleanLogoData(JsonNode? node,HashSet<string> removed) {
        if(node is JsonObject obj)foreach(var key in obj.Select(p=>p.Key).ToArray()) {
            var child=obj[key];
            if(child is JsonObject reference && reference["ref"] is JsonValue value && value.TryGetValue<string>(out var id) && removed.Contains(id))obj[key]=null;
            else CleanLogoData(child,removed);
        } else if(node is JsonArray array)for(int i=array.Count-1;i>=0;i--) {
            var child=array[i];
            if(child is JsonObject entry && (entry["ref"] is JsonValue value && value.TryGetValue<string>(out var id) && removed.Contains(id) || LogoPath(entry["RendererPath"]?.GetValue<string>()) || LogoPath(entry["path"]?.GetValue<string>())))array.RemoveAt(i);
            else CleanLogoData(child,removed);
        }
    }
    static void VerifyNoCenterLoginLogos(string root) {
        foreach(var path in Directory.GetFiles(LoginContentDirectory(root),"scene.json",SearchOption.AllDirectories)) {
            var scene=JsonNode.Parse(File.ReadAllText(path))!;
            if(scene["objects"]!.AsArray().Any(row=>row!=null && (SoulWeaponLoginSprite(row) || row["kind"]?.GetValue<string>()=="GameObject" && CenterLoginLogoName.IsMatch(row["name"]!.GetValue<string>()))))
                throw new InvalidOperationException("Large login logo remains in scene data: "+path);
        }
        Console.WriteLine("Verified historical title and Soul Weapon overlay objects are removed; the native corner logo is unchanged.");
    }
    static void RemoveCenterLoginLogos(string root) {
        var content=LoginContentDirectory(root);int count=0;
        foreach(var path in Directory.GetFiles(content,"scene.json",SearchOption.AllDirectories)) {
            var scene=JsonNode.Parse(File.ReadAllText(path))!;var rows=scene["objects"]!.AsArray();
            var objects=rows.ToDictionary(row=>row!["id"]!.GetValue<string>(),row=>row!);
            var removedGo=objects.Values.Where(row=>row["kind"]!.GetValue<string>()=="GameObject" && CenterLoginLogoName.IsMatch(row["name"]!.GetValue<string>())).Select(row=>row["id"]!.GetValue<string>()).ToHashSet();
            foreach(var sprite in objects.Values.Where(SoulWeaponLoginSprite))removedGo.UnionWith(LoginReferences(sprite["go"]));
            if(removedGo.Count==0)continue;
            var removed=new HashSet<string>(removedGo);
            bool changed=true;
            while(changed) {
                int before=removedGo.Count;
                foreach(var row in objects.Values.Where(row=>row["kind"]!.GetValue<string>()=="Transform"))
                    if(LoginReferences(row["parent"]).Any(removed.Contains))removedGo.UnionWith(LoginReferences(row["go"]));
                removed.UnionWith(removedGo);
                foreach(var id in removedGo)removed.UnionWith(LoginReferences(objects[id]["components"]));
                changed=before!=removedGo.Count;
            }
            // Effects instantiate standalone prefab assets by name. Remove those
            // entries as well as renderer paths inside combined art/logo models.
            foreach(var row in objects.Values.Where(row=>row["type"]?.GetValue<string>()=="NGameAsset.NEffect.OpenDataFromResource")) {
                var data=row["data"]!;var names=new HashSet<string>();var assets=data["NamedAssetList"]?.AsArray();
                if(assets!=null)for(int i=assets.Count-1;i>=0;i--)if(LoginReferences(assets[i]?["Asset"]).Any(removed.Contains)) {
                    var name=assets[i]?["Name"]?.GetValue<string>();if(!String.IsNullOrEmpty(name))names.Add(name);assets.RemoveAt(i);
                }
                var textId=LoginReferences(data["JsonText"]).FirstOrDefault();
                if(textId==null || !objects.TryGetValue(textId,out var text))continue;
                var file=Path.Combine(Path.GetDirectoryName(path)!,text["file"]!.GetValue<string>());
                var effect=JsonNode.Parse(File.ReadAllText(file))!;
                foreach(var list in new[]{"ModelList","ParticleList","TrailList","SoundFxList"})if(effect[list] is JsonArray entries)
                    for(int i=entries.Count-1;i>=0;i--)if(names.Contains(entries[i]?["Name"]?.GetValue<string>()??""))entries.RemoveAt(i);
                string before=effect.ToJsonString();CleanLogoData(effect,removed);
                if(names.Count>0 || effect.ToJsonString()!=before)File.WriteAllText(file,effect.ToJsonString(new JsonSerializerOptions{WriteIndented=true})+"\n");
            }
            CleanLogoData(scene,removed);
            // Keep only objects reachable from the surviving scene/prefabs. This
            // removes logo meshes, materials, textures and animation dependencies.
            var reachable=new HashSet<string>();var queue=new Queue<string>(LoginReferences(scene["root"]).Concat(LoginReferences(scene["attachments"])));
            while(queue.TryDequeue(out var id))if(!removed.Contains(id) && reachable.Add(id) && objects.TryGetValue(id,out var row))foreach(var child in LoginReferences(row))queue.Enqueue(child);
            var oldFiles=objects.Values.Where(row=>row["file"]!=null).Select(row=>row["file"]!.GetValue<string>()).ToHashSet();
            var retained=objects.Where(pair=>reachable.Contains(pair.Key) && !removed.Contains(pair.Key)).Select(pair=>pair.Value).ToArray();
            var files=retained.Where(row=>row["file"]!=null).Select(row=>row["file"]!.GetValue<string>()).ToHashSet();
            for(int i=rows.Count-1;i>=0;i--)if(!reachable.Contains(rows[i]!["id"]!.GetValue<string>()) || removed.Contains(rows[i]!["id"]!.GetValue<string>()))rows.RemoveAt(i);
            File.WriteAllText(path,scene.ToJsonString()+"\n");
            foreach(var file in oldFiles.Except(files)) {
                if(!Regex.IsMatch(file,@"^assets/[0-9a-f]{24}\.(png|txt)$"))throw new InvalidOperationException("Unexpected generated asset path: "+file);
                File.Delete(Path.Combine(Path.GetDirectoryName(path)!,file));
            }
            count+=removedGo.Count;
        }
        var integrity=new JsonObject();
        foreach(var path in Directory.GetFiles(content,"*",SearchOption.AllDirectories).OrderBy(p=>p)) {
            if(Path.GetFileName(path)=="integrity.json")continue;
            using var stream=File.OpenRead(path);
            integrity[Path.GetRelativePath(content,path).Replace('\\','/')]=new JsonObject{{"size",stream.Length},{"sha256",Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant()}};
        }
        File.WriteAllText(Path.Combine(content,"integrity.json"),integrity.ToJsonString(new JsonSerializerOptions{WriteIndented=true})+"\n");
        VerifyNoCenterLoginLogos(root);Console.WriteLine($"Removed {count} large login logo objects and their unused dependencies.");
    }
}
