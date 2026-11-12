using System;
using LitJson;
using NGame2.NNodeTree.NRuntime;

namespace NGame2.NNodeTree.NLink;

public class Callee : LinkBase, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	private Action<RuntimeInstance, IVariable> _calledAction;

	public bool FlipGUI;

	public bool DisableThis { get; set; }

	public Callee(ContentsBase contents, string name, Action<RuntimeInstance, IVariable> calledAction)
		: base(contents, name)
	{
		_calledAction = calledAction;
	}

	public void Call(RuntimeInstance runtime, IVariable variable)
	{
		if (!DisableThis)
		{
			Vespa_System_Action_Extension_Method.fire(_calledAction, runtime, variable);
		}
	}

	bool ILoadJson.Load(JsonData i_json)
	{
		if (!i_json.IsObject())
		{
			return false;
		}
		DisableThis = i_json["DisableThis"].GETBOOLEAN(DisableThis);
		FlipGUI = i_json["FlipGUI"].GETBOOLEAN(FlipGUI);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("DisableThis", DisableThis);
		jsonData.AddFieldBool("FlipGUI", FlipGUI);
		return jsonData;
	}
}
