using System.Collections.Generic;
using LitJson;
using NGame2.NNodeTree.NContents;
using NGame2.NNodeTree.NRuntime;
using NGame2.NScene;

namespace NGame2.NNodeTree;

public class TreeData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public const string JsonPrefix = "[nodetree_treedata_json_prefix]";

	public string Code = string.Empty;

	public bool SkipCaching;

	public List<NodeData> NodeList = new List<NodeData>();

	public RootBase ContentsRoot;

	public PackData Pack { get; private set; }

	public TreeData(PackData pack)
	{
		Pack = pack;
	}

	public NodeData GetNode(int nodeId)
	{
		for (int i = 0; i < NodeList.Count; i++)
		{
			NodeData nodeData = NodeList[i];
			if (nodeData.NodeId == nodeId)
			{
				return nodeData;
			}
		}
		return null;
	}

	public void ReserveCache(IResourcePreloader preloader)
	{
		if (SkipCaching)
		{
			return;
		}
		for (int i = 0; i < NodeList.Count; i++)
		{
			NodeData nodeData = NodeList[i];
			if (nodeData.Contents != null)
			{
				nodeData.Contents.ReserveCache(preloader);
			}
		}
	}

	public void Wakeup(IVariable variable)
	{
		for (int i = 0; i < NodeList.Count; i++)
		{
			NodeData nodeData = NodeList[i];
			if (nodeData.Contents != null)
			{
				nodeData.Contents.Wakeup(variable);
			}
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		Code = json["Code"].GETSTRING(Code);
		SkipCaching = json["SkipCaching"].GETBOOLEAN(SkipCaching);
		JsonData jsonData = json["NodeArray"];
		if (jsonData.IsArray())
		{
			for (int i = 0; i < jsonData.Count; i++)
			{
				JsonData fromArray = jsonData.GetFromArray(i);
				NodeData nodeData = new NodeData(this);
				if (nodeData.LOADFROMJSON(fromArray))
				{
					if (nodeData.Contents is RootBase contentsRoot)
					{
						ContentsRoot = contentsRoot;
					}
					NodeList.Add(nodeData);
				}
			}
		}
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("Code", Code);
		jsonData.AddFieldBool("SkipCaching", SkipCaching);
		JsonData jsonData2 = new JsonData();
		for (int i = 0; i < NodeList.Count; i++)
		{
			JsonData data = NodeList[i].MAKEJSON();
			jsonData2.Add(data);
		}
		jsonData.AddField("NodeArray", jsonData2);
		return jsonData;
	}
}
