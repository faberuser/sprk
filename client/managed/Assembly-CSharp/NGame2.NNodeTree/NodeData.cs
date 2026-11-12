using LitJson;
using NGame2.NNodeTree.NContents;
using NVespa;

namespace NGame2.NNodeTree;

public class NodeData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public const string JsonPrefix = "[nodetree_node_json_prefix]";

	public int NodeId;

	public ContentsBase Contents;

	public TreeData Tree { get; private set; }

	public NodeData(TreeData tree)
	{
		Tree = tree;
	}

	bool ILoadJson.Load(JsonData json)
	{
		JsonData jsonAny = json.GetJsonAny("NodeId", "node_id");
		if (jsonAny.IsLong())
		{
			NodeId = jsonAny.valueInt;
		}
		JsonData jsonAny2 = json.GetJsonAny("Contents", "contents");
		if (jsonAny2.IsObject())
		{
			Contents = SingletonAuto<Factory>.instance.CreateFromJson(jsonAny2, this);
		}
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldLong("NodeId", NodeId);
		if (Contents != null)
		{
			jsonData.AddField("Contents", Contents.MakeJson());
		}
		return jsonData;
	}
}
