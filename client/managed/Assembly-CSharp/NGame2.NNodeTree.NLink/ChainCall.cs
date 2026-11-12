using System;
using LitJson;
using NGame2.NNodeTree.NRuntime;
using NGameAsset;

namespace NGame2.NNodeTree.NLink;

public class ChainCall : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public EnableSpecData EnableSpec = new EnableSpecData();

	public Callee CalleeStart;

	public Caller CallerChain;

	public Caller CallerEnd;

	private Func<RuntimeInstance, IVariable, IVariable> _calledAction;

	public ChainCall(ContentsBase i_contents, string i_prefix, Func<RuntimeInstance, IVariable, IVariable> calledAction)
	{
		_calledAction = calledAction;
		CalleeStart = new Callee(i_contents, i_prefix + "CalleeStart", CallAction);
		CallerChain = new Caller(i_contents, i_prefix + "CallerChain");
		CallerEnd = new Caller(i_contents, i_prefix + "CallerEnd");
	}

	public void EndCall(RuntimeInstance runtime, IVariable variable)
	{
		CallerEnd.Call(runtime, variable);
	}

	private void CallAction(RuntimeInstance runtime, IVariable variable)
	{
		IVariable variable2 = variable;
		if (EnableSpec.CheckEnable())
		{
			IVariable variable3 = Vespa_System_Action_Extension_Method.fire(_calledAction, runtime, variable);
			if (variable3 != null)
			{
				variable2 = variable3;
			}
		}
		CallerChain.Call(runtime, variable2);
	}

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		CalleeStart.LOADFROMJSON(json["CalleeStart"]);
		CallerChain.LOADFROMJSON(json["CallerChain"]);
		CallerEnd.LOADFROMJSON(json["CallerEnd"]);
		EnableSpec.LOADFROMJSON(json["EnableSpec"]);
		JsonData i_json = json["DisableThis"];
		if (i_json.IsBoolean())
		{
			bool bOOLEAN = i_json.GETBOOLEAN(i_default: false);
			EnableSpec.EnableHigh = !bOOLEAN;
			EnableSpec.EnableMid = !bOOLEAN;
			EnableSpec.EnableLow = !bOOLEAN;
		}
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldJsonMake("CalleeStart", CalleeStart);
		jsonData.AddFieldJsonMake("CallerChain", CallerChain);
		jsonData.AddFieldJsonMake("CallerEnd", CallerEnd);
		jsonData.AddFieldJsonMake("EnableSpec", EnableSpec);
		return jsonData;
	}
}
