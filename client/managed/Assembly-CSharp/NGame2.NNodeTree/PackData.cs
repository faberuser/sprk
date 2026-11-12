using System.Collections.Generic;
using LitJson;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NUtil;
using NGame2.NScene;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree;

public class PackData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public class UidTree
	{
		public long Uid;

		public TreeData Tree;
	}

	public string PathName = string.Empty;

	public List<UidTree> TreeList = new List<UidTree>();

	public List<LinkNodeTree2> LinkList = new List<LinkNodeTree2>();

	public void ReserveCache(IResourcePreloader preloader)
	{
		for (int i = 0; i < TreeList.Count; i++)
		{
			TreeList[i].Tree?.ReserveCache(preloader);
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(LinkList); j++)
		{
			LinkNodeTree2 linkNodeTree = LinkList[j];
			preloader.AddNodeTree(linkNodeTree.PackPathName, linkNodeTree.TreeUid);
		}
	}

	public long CreateNewUid()
	{
		long num = Random.Range(0, int.MaxValue);
		while (true)
		{
			bool flag = false;
			for (int i = 0; i < TreeList.Count; i++)
			{
				UidTree uidTree = TreeList[i];
				if (num == uidTree.Uid)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				break;
			}
			num++;
		}
		return num;
	}

	public bool ContainsTree(TreeData tree)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(TreeList); i++)
		{
			if (Vespa_Common_Class_Extension_Method.EQUALS(TreeList[i].Tree, tree))
			{
				return true;
			}
		}
		return false;
	}

	public TreeData GetNodeTree(long uid)
	{
		for (int i = 0; i < TreeList.Count; i++)
		{
			UidTree uidTree = TreeList[i];
			if (uidTree.Uid == uid)
			{
				return uidTree.Tree;
			}
		}
		return null;
	}

	public TreeData GetNodeTree(string code)
	{
		for (int i = 0; i < TreeList.Count; i++)
		{
			UidTree uidTree = TreeList[i];
			if (uidTree.Tree.Code.Equals(code))
			{
				return uidTree.Tree;
			}
		}
		return null;
	}

	private void Wakeup()
	{
		LocalVariable localVariable = SmallPooling<LocalVariable>.CreateInstance();
		for (int i = 0; i < TreeList.Count; i++)
		{
			TreeList[i].Tree.Wakeup(localVariable);
		}
		SmallPooling<LocalVariable>.ReleaseInstance(localVariable);
	}

	bool ILoadJson.Load(JsonData i_json)
	{
		return LoadFromJson(i_json);
	}

	private bool LoadFromJson(JsonData i_json)
	{
		if (!i_json.IsObject())
		{
			return false;
		}
		JsonData jsonAny = i_json.GetJsonAny("TreeArray", "NodeTreeArray", "nodetree_array");
		if (jsonAny.IsArray())
		{
			for (int i = 0; i < jsonAny.Count; i++)
			{
				JsonData fromArray = jsonAny.GetFromArray(i);
				JsonData i_json2 = fromArray["Uid"];
				JsonData json = fromArray["Body"];
				if (i_json2.isNone())
				{
					TreeData treeData = new TreeData(this);
					if (!treeData.LOADFROMJSON(fromArray))
					{
						return false;
					}
					UidTree uidTree = new UidTree();
					uidTree.Uid = CreateNewUid();
					uidTree.Tree = treeData;
					TreeList.Add(uidTree);
				}
				else
				{
					UidTree uidTree2 = new UidTree();
					uidTree2.Uid = i_json2.GETLONG(uidTree2.Uid);
					uidTree2.Tree = new TreeData(this);
					if (!uidTree2.Tree.LOADFROMJSON(json))
					{
						return false;
					}
					TreeList.Add(uidTree2);
				}
			}
		}
		LinkList.LoadFromJson(i_json["LinkList"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		JsonData jsonData2 = new JsonData();
		for (int i = 0; i < TreeList.Count; i++)
		{
			UidTree uidTree = TreeList[i];
			JsonData jsonData3 = new JsonData();
			jsonData3.AddFieldLong("Uid", uidTree.Uid);
			jsonData3.AddField("Body", uidTree.Tree.MAKEJSON());
			jsonData2.Add(jsonData3);
		}
		jsonData.AddField("TreeArray", jsonData2);
		jsonData.AddFieldList("LinkList", LinkList);
		return jsonData;
	}
}
