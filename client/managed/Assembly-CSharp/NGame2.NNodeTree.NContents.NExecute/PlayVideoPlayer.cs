using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NUI.NWindow;
using NShared;

namespace NGame2.NNodeTree.NContents.NExecute;

public class PlayVideoPlayer : ExecuteBase, IKey
{
	public static string ContentsTypeName = "Execute/" + typeof(PlayVideoPlayer).Name;

	public Callee CalleeStart;

	public Caller CallerChain;

	public Caller CallerEnd;

	public ArgumentSingleGeneric<string> Code;

	public PlayVideoPlayer(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "ContentsStart", ActionStart);
		CallerChain = new Caller(this, "CallerChain");
		CallerEnd = new Caller(this, "EndCaller");
		Code = new ArgumentSingleGeneric<string>(string.Empty);
	}

	public override void Wakeup(IVariable variable)
	{
		Code.GetValue(variable);
	}

	public void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		string value = Code.GetValue(variable);
		if (!String_Extension_Method.IsNullOrEmpty(GameRoot.Instance.VideoDataContainer.GetVideoPath(value)))
		{
			VideoManager.Open_VideoCode(value);
		}
		else
		{
			VideoManager.Open_Path(value);
		}
		CallerChain.Call(runtime, variable);
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		CalleeStart.LOADFROMJSON(json["CalleeStart"]);
		CallerChain.LOADFROMJSON(json["CallerChain"]);
		Code.LoadFromJson(json["Code"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldJsonMake("CalleeStart", CalleeStart);
		jsonData.AddFieldJsonMake("CallerChain", CallerChain);
		jsonData.AddField("Code", Code.MakeJson());
		return jsonData;
	}
}
