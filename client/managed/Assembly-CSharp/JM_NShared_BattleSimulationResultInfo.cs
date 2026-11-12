using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleSimulationResultInfo
{
	public static BattleSimulationResultInfo Parse(string s)
	{
		return Parse(s, new BattleSimulationResultInfo());
	}

	public static BattleSimulationResultInfo Parse(string s, BattleSimulationResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleSimulationResultInfo Parse(IDictionary d)
	{
		return Parse(d, new BattleSimulationResultInfo());
	}

	public static BattleSimulationResultInfo Parse(IDictionary d, BattleSimulationResultInfo o)
	{
		o.GroupId = BaseJsonMarshaler.ParseValueString(d, "GroupId");
		o.Uid = BaseJsonMarshaler.ParseValueString(d, "Uid");
		o.IsWin = BaseJsonMarshaler.ParseValueBool(d, "IsWin");
		o.RealTime = BaseJsonMarshaler.ParseValueLong(d, "RealTime");
		return o;
	}

	public static string EncodeJsonString(BattleSimulationResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleSimulationResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.GroupId != null)
		{
			dictionary.Add("GroupId", o.GroupId);
		}
		if (o.Uid != null)
		{
			dictionary.Add("Uid", o.Uid);
		}
		dictionary.Add("IsWin", o.IsWin.ToString());
		dictionary.Add("RealTime", o.RealTime.ToString());
		return dictionary;
	}
}
