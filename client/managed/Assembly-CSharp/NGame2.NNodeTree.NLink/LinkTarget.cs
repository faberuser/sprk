using LitJson;

namespace NGame2.NNodeTree.NLink;

public class LinkTarget : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public int NodeId = -1;

	public string LinkName = string.Empty;

	public bool IsCached;

	public LinkBase LinkCache;

	private LinkBase _ownerLink;

	public LinkTarget(LinkBase ownerLink)
	{
		_ownerLink = ownerLink;
	}

	public NodeData GetNode()
	{
		if (_ownerLink == null)
		{
			return null;
		}
		NodeData ownerNode = _ownerLink.OwnerNode;
		if (ownerNode == null || ownerNode.Tree == null)
		{
			return null;
		}
		return ownerNode.Tree.GetNode(NodeId);
	}

	public LinkBase GetLink()
	{
		if (NodeId < 0)
		{
			return null;
		}
		NodeData node = GetNode();
		if (node == null)
		{
			return null;
		}
		if (node.Contents == null)
		{
			return null;
		}
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(node.Contents.LinkDic, LinkName, null);
	}

	public LinkType GetLink<LinkType>() where LinkType : LinkBase
	{
		if (IsCached)
		{
			return LinkCache as LinkType;
		}
		LinkBase linkBase = (LinkCache = GetLink());
		IsCached = true;
		return linkBase as LinkType;
	}

	bool ILoadJson.Load(JsonData i_json)
	{
		NodeId = i_json["NodeId"].GETINT(NodeId);
		LinkName = i_json["LinkName"].GETSTRING(LinkName);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldLong("NodeId", NodeId);
		jsonData.AddFieldString("LinkName", LinkName);
		return jsonData;
	}
}
