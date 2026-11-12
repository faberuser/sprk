using LitJson;
using NGame2.NCutScene;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NVespa.NSingleton;

namespace NGame2.NNodeTree.NContents.NExecute;

public class PlayCutScene : ExecuteBase, IKey
{
	public static string ContentsTypeName = "Execute/" + typeof(PlayCutScene).Name;

	public Callee CalleeStart;

	public Caller CallerChain;

	public ArgumentSingleGeneric<string> Code;

	public PlayCutScene(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "ContentsStart", ActionStart);
		CallerChain = new Caller(this, "CallerChain");
		Code = new ArgumentSingleGeneric<string>(string.Empty);
	}

	public override void Wakeup(IVariable variable)
	{
		Code.GetValue(variable);
	}

	public void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		string value = Code.GetValue(variable);
		SceneAutoBehaviour<CutSceneManager>.instance.Open(value, canSkip: true, null);
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
