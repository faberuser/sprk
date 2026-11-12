using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleSimulationResultInfoWrapper
{
	public static BattleSimulationResultInfoWrapper Parse(string s)
	{
		return Parse(s, new BattleSimulationResultInfoWrapper());
	}

	public static BattleSimulationResultInfoWrapper Parse(string s, BattleSimulationResultInfoWrapper o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleSimulationResultInfoWrapper Parse(IDictionary d)
	{
		return Parse(d, new BattleSimulationResultInfoWrapper());
	}

	public static BattleSimulationResultInfoWrapper Parse(IDictionary d, BattleSimulationResultInfoWrapper o)
	{
		o.BattleSimulationResultInfos = BaseJsonMarshaler.ParseArray(d, "BattleSimulationResultInfos", JM_NShared_BattleSimulationResultInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(BattleSimulationResultInfoWrapper o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleSimulationResultInfoWrapper o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BattleSimulationResultInfos", BaseJsonMarshaler.EncodeArray(o.BattleSimulationResultInfos, JM_NShared_BattleSimulationResultInfo.EncodeJsonObject));
		return dictionary;
	}
}
