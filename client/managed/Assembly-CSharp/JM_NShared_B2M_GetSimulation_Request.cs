using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.B2M_GetSimulation;

public static class JM_NShared_B2M_GetSimulation_Request
{
	public static NShared.B2M_GetSimulation.Request Parse(string s)
	{
		return Parse(s, new NShared.B2M_GetSimulation.Request());
	}

	public static NShared.B2M_GetSimulation.Request Parse(string s, NShared.B2M_GetSimulation.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.B2M_GetSimulation.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.B2M_GetSimulation.Request());
	}

	public static NShared.B2M_GetSimulation.Request Parse(IDictionary d, NShared.B2M_GetSimulation.Request o)
	{
		o.BattleSimulationResultInfos = BaseJsonMarshaler.ParseValueString(d, "BattleSimulationResultInfos");
		o.ReservedSimulationCount = BaseJsonMarshaler.ParseValueInt(d, "ReservedSimulationCount");
		return o;
	}

	public static string EncodeJsonString(NShared.B2M_GetSimulation.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.B2M_GetSimulation.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.BattleSimulationResultInfos != null)
		{
			dictionary.Add("BattleSimulationResultInfos", o.BattleSimulationResultInfos);
		}
		dictionary.Add("ReservedSimulationCount", o.ReservedSimulationCount.ToString());
		return dictionary;
	}
}
