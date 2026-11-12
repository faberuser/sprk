using LitJson;
using NGame2.NNodeTree.NRuntime;
using NVespa;

namespace NGame2.NNodeTree.NLink;

public class Caller : LinkBase, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool FlipGUI;

	public PoolingList<LinkTarget> LinkTargetList;

	public bool DisableThis { get; set; }

	public Caller(ContentsBase contents, string name)
		: base(contents, name)
	{
	}

	bool ILoadJson.Load(JsonData i_json)
	{
		if (!i_json.IsObject())
		{
			return false;
		}
		DisableThis = i_json["DisableThis"].GETBOOLEAN(DisableThis);
		FlipGUI = i_json["FlipGUI"].GETBOOLEAN(FlipGUI);
		JsonData jsonAny = i_json.GetJsonAny("LinkTargetArray", "LinkTargetList");
		if (jsonAny.IsArray())
		{
			for (int i = 0; i < jsonAny.Count; i++)
			{
				JsonData fromArray = jsonAny.GetFromArray(i);
				LinkTarget linkTarget = new LinkTarget(this);
				linkTarget.LOADFROMJSON(fromArray);
				LinkTargetList.Add(linkTarget);
			}
		}
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		for (int i = 0; i < LinkTargetList.Count; i++)
		{
			JsonData data = LinkTargetList[i].MAKEJSON();
			jsonData.Add(data);
		}
		JsonData jsonData2 = new JsonData();
		jsonData2.AddFieldBool("DisableThis", DisableThis);
		jsonData2.AddFieldBool("FlipGUI", FlipGUI);
		jsonData2.AddField("LinkTargetArray", jsonData);
		return jsonData2;
	}

	public void Call(RuntimeInstance i_runtime, IVariable variable)
	{
		if (i_runtime.IsStoped || DisableThis)
		{
			return;
		}
		for (int i = 0; i < LinkTargetList.Count; i++)
		{
			LinkTarget linkTarget = LinkTargetList[i];
			if (linkTarget.NodeId >= 0)
			{
				linkTarget.GetLink<Callee>()?.Call(i_runtime, variable);
			}
		}
	}
}
