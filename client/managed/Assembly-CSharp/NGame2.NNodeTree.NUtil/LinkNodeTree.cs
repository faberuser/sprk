using System.Collections.Generic;
using LitJson;
using NGame2.NNodeTree.NContents.NRoot;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NScene;
using NVespa;

namespace NGame2.NNodeTree.NUtil;

public class LinkNodeTree
{
	public string PackPathName = string.Empty;

	public long TreeUid;

	public string Code = string.Empty;

	public IncreaseDic<StringKey, ArgumentSingle> MappingDic = new IncreaseDic<StringKey, ArgumentSingle>(StringKey.Comparer);

	public ChildRoot GetChildRoot()
	{
		return GetChildRoot(ifExist: false);
	}

	public ChildRoot GetChildRoot(bool ifExist)
	{
		TreeData tree = GetTree(ifExist);
		if (tree == null)
		{
			return null;
		}
		return tree.ContentsRoot as ChildRoot;
	}

	public TreeData GetTree()
	{
		return GetTree(ifExist: false);
	}

	public TreeData GetTree(bool ifExist)
	{
		PackData pack = GetPack(ifExist);
		if (pack == null)
		{
			return null;
		}
		for (int i = 0; i < pack.TreeList.Count; i++)
		{
			PackData.UidTree uidTree = pack.TreeList[i];
			if (TreeUid == uidTree.Uid)
			{
				return uidTree.Tree;
			}
		}
		return null;
	}

	private PackData GetPack(bool ifExist)
	{
		if (String_Extension_Method.IsNullOrEmpty(PackPathName))
		{
			return null;
		}
		PackLoad packLoad = ((!ifExist) ? SingletonAuto<PackContainer>.instance.GetPack(PackPathName) : SingletonAuto<PackContainer>.instance.GetExistPack(PackPathName));
		if (!packLoad.ISDONE() || !packLoad.ISVALID())
		{
			return null;
		}
		return packLoad.Value;
	}

	public void ReserveCache(IResourcePreloader preloader)
	{
		preloader.AddNodeTree(PackPathName, TreeUid);
	}

	public bool LoadFromJson(JsonData json, ContentsBase contents)
	{
		if (!json.IsObject())
		{
			return false;
		}
		PackPathName = json.GetJsonAny("PackPathName", "PackPath").GETSTRING(PackPathName);
		Code = json["Code"].GETSTRING(Code);
		TreeUid = json["TreeUid"].GETLONG(TreeUid);
		JsonData jsonData = json["MappingArray"];
		if (jsonData.IsArray())
		{
			for (int i = 0; i < jsonData.Count; i++)
			{
				JsonData fromArray = jsonData.GetFromArray(i);
				StringKey stringKey = StringKey.CreateFromJson(fromArray["Key"]);
				if (!stringKey.IsNullOrEmpty())
				{
					ArgumentSingle argumentSingle = new ArgumentSingle();
					argumentSingle.LoadFromJson(fromArray["ArgumentSingle"]);
					MappingDic.Add(stringKey, argumentSingle);
				}
			}
		}
		return true;
	}

	public JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("PackPathName", PackPathName);
		jsonData.AddFieldString("Code", Code);
		jsonData.AddFieldLong("TreeUid", TreeUid);
		jsonData.AddField("MappingArray", MakeJsonArray_From_MappingDic());
		return jsonData;
	}

	private JsonData MakeJsonArray_From_MappingDic()
	{
		if (MappingDic.IsNullOrEmpty())
		{
			return null;
		}
		ChildRoot childRoot = GetChildRoot(ifExist: true);
		JsonData jsonData = new JsonData();
		for (int i = 0; i < MappingDic.Count; i++)
		{
			KeyValuePair<StringKey, ArgumentSingle> keyValuePairByIndex = MappingDic.GetKeyValuePairByIndex(i);
			StringKey mappingKey = keyValuePairByIndex.Key;
			ArgumentSingle value = keyValuePairByIndex.Value;
			if (childRoot == null || childRoot.MemberList.Find((ChildRoot.MemberVariable memberVar) => Vespa_Common_Class_Extension_Method.EQUALS(mappingKey, memberVar.Name)) != null)
			{
				JsonData jsonData2 = new JsonData();
				jsonData2.AddField("Key", mappingKey.MakeJson());
				jsonData2.AddField("ArgumentSingle", value.MakeJson());
				jsonData.Add(jsonData2);
			}
		}
		return jsonData;
	}
}
