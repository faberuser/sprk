using LitJson;
using NGame2.NFunctionality;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NVespa.NSingleton;

namespace NGame2.NNodeTree.NContents.NExecute;

public class SetFunctionality : ExecuteBase, IKey
{
	public static string ContentsTypeName = "Execute/" + typeof(SetFunctionality).Name;

	public Callee CalleeStart;

	public Caller CallerEnd;

	public FunctionalityType Type;

	public ArgumentSingleGeneric<string> Value;

	public SetFunctionality(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "ContentsStart", ActionStart);
		CallerEnd = new Caller(this, "EndCaller");
		Value = new ArgumentSingleGeneric<string>(string.Empty);
	}

	public override void Wakeup(IVariable variable)
	{
		Value.GetValue(variable);
	}

	private void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		GlobalAutoBehaviour<FunctionalityManager>.instance.SetValue(Type, Value.GetValue(variable));
		CallerEnd.Call(runtime, variable);
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		CalleeStart.LOADFROMJSON(json["CalleeStart"]);
		CallerEnd.LOADFROMJSON(json["CallerEnd"]);
		Type = json["Type"].GETENUM(Type);
		Value.LoadFromJson(json["Value"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("CalleeStart", CalleeStart.MAKEJSON());
		jsonData.AddField("CallerEnd", CallerEnd.MAKEJSON());
		jsonData.AddFieldEnum("Type", Type);
		jsonData.AddField("Value", Value.MakeJson());
		return jsonData;
	}
}
