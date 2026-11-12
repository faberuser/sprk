using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BaseBattleLog
{
	public static BaseBattleLog Parse(string s)
	{
		return Parse(s, new BaseBattleLog());
	}

	public static BaseBattleLog Parse(string s, BaseBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BaseBattleLog Parse(IDictionary d)
	{
		return Parse(d, new BaseBattleLog());
	}

	public static BaseBattleLog Parse(IDictionary d, BaseBattleLog o)
	{
		o.No = BaseJsonMarshaler.ParseValueInt(d, "No");
		o.GlobalTimeMs = BaseJsonMarshaler.ParseValueLong(d, "GlobalTimeMs");
		o.Type = BaseJsonMarshaler.ParseEnum<BattleLogType>(d, "Type");
		o.Data = BaseJsonMarshaler.ParseValueString(d, "Data");
		return o;
	}

	public static string EncodeJsonString(BaseBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BaseBattleLog o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("No", o.No.ToString());
		dictionary.Add("GlobalTimeMs", o.GlobalTimeMs.ToString());
		dictionary.Add("Type", o.Type.ToString());
		if (o.Data != null)
		{
			dictionary.Add("Data", o.Data);
		}
		return dictionary;
	}
}
