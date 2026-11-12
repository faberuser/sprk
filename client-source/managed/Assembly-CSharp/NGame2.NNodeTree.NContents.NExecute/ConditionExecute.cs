using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;

namespace NGame2.NNodeTree.NContents.NExecute;

public class ConditionExecute : ExecuteBase, IKey
{
	public static string ContentsTypeName = "Execute/" + typeof(ConditionExecute).Name;

	public Callee CalleeStart;

	public Caller CallerIfTrue;

	public Caller CallerIfFalse;

	public ArgumentSingleGeneric<bool> Condition;

	public ConditionExecute(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "CalleeStart", ActionStart);
		CallerIfTrue = new Caller(this, "CallerIfTrue");
		CallerIfFalse = new Caller(this, "CallerIfFalse");
		Condition = new ArgumentSingleGeneric<bool>(value: true);
	}

	public override void Wakeup(IVariable variable)
	{
		Condition.GetValue(variable);
	}

	public void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		if (Condition.GetValue(variable, defaultValue: true))
		{
			CallerIfTrue.Call(runtime, variable);
		}
		else
		{
			CallerIfFalse.Call(runtime, variable);
		}
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		CalleeStart.LOADFROMJSON(json["CalleeStart"]);
		CallerIfTrue.LOADFROMJSON(json["CallerIfTrue"]);
		CallerIfFalse.LOADFROMJSON(json["CallerIfFalse"]);
		Condition.LoadFromJson(json["Condition"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldJsonMake("CalleeStart", CalleeStart);
		jsonData.AddFieldJsonMake("CallerIfTrue", CallerIfTrue);
		jsonData.AddFieldJsonMake("CallerIfFalse", CallerIfFalse);
		jsonData.AddField("Condition", Condition.MakeJson());
		return jsonData;
	}
}
