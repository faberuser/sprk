using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_BattleSpeedChange
{
	public static BattleSpeedChange Parse(string s)
	{
		return Parse(s, new BattleSpeedChange());
	}

	public static BattleSpeedChange Parse(string s, BattleSpeedChange o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleSpeedChange Parse(IDictionary d)
	{
		return Parse(d, new BattleSpeedChange());
	}

	public static BattleSpeedChange Parse(IDictionary d, BattleSpeedChange o)
	{
		o.BattleGlobalTime = BaseJsonMarshaler.ParseValueFloat(d, "BattleGlobalTime");
		o.SpeedRate = BaseJsonMarshaler.ParseValueFloat(d, "SpeedRate");
		return o;
	}

	public static string EncodeJsonString(BattleSpeedChange o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleSpeedChange o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BattleGlobalTime", o.BattleGlobalTime.ToString());
		dictionary.Add("SpeedRate", o.SpeedRate.ToString());
		return dictionary;
	}
}
