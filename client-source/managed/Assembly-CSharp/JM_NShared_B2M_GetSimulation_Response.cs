using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.B2M_GetSimulation;

public static class JM_NShared_B2M_GetSimulation_Response
{
	public static NShared.B2M_GetSimulation.Response Parse(string s)
	{
		return Parse(s, new NShared.B2M_GetSimulation.Response());
	}

	public static NShared.B2M_GetSimulation.Response Parse(string s, NShared.B2M_GetSimulation.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.B2M_GetSimulation.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.B2M_GetSimulation.Response());
	}

	public static NShared.B2M_GetSimulation.Response Parse(IDictionary d, NShared.B2M_GetSimulation.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.RemoveGroupIds = BaseJsonMarshaler.ParseArrayCast(d, "RemoveGroupIds", BaseJsonMarshaler.CastToString);
		o.BattleSimulationInfos = BaseJsonMarshaler.ParseArray(d, "BattleSimulationInfos", JM_NShared_BattleSimulationInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(NShared.B2M_GetSimulation.Response o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.B2M_GetSimulation.Response o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Result", o.Result.ToString());
		dictionary.Add("RemoveGroupIds", BaseJsonMarshaler.EncodeArray(o.RemoveGroupIds));
		dictionary.Add("BattleSimulationInfos", BaseJsonMarshaler.EncodeArray(o.BattleSimulationInfos, JM_NShared_BattleSimulationInfo.EncodeJsonObject));
		return dictionary;
	}
}
