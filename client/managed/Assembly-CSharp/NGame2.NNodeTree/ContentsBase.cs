using System.Collections.Generic;
using LitJson;
using NGame2.NNodeTree.NRuntime;
using NGame2.NScene;

namespace NGame2.NNodeTree;

public abstract class ContentsBase : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public enum EStopContentsType
	{
		StopWithThis,
		StopIfRequestStop,
		StopIfBattleCancel,
		None
	}

	public Dictionary<string, LinkBase> LinkDic = new Dictionary<string, LinkBase>();

	public NodeData Node { get; private set; }

	public ContentsBase(NodeData node)
	{
		Node = node;
	}

	public string GetContentsPath()
	{
		string text = GetContentsTypeName();
		TreeData tree = this.GetNode().GetTree();
		if (tree != null)
		{
			text = tree.Code + "." + text;
		}
		PackData pack = tree.GetPack();
		if (pack != null)
		{
			text = pack.PathName + "." + text;
		}
		return text;
	}

	public virtual void ReserveCache(IResourcePreloader preloader)
	{
	}

	public abstract void Wakeup(IVariable variable);

	bool ILoadJson.Load(JsonData json)
	{
		LoadFromJson(json);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		return MakeJson();
	}

	public abstract void LoadFromJson(JsonData i_json);

	public abstract JsonData MakeJson();

	internal abstract string GetContentsTypeName();
}
