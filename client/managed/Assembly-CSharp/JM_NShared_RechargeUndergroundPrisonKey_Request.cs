using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RechargeUndergroundPrisonKey;

public static class JM_NShared_RechargeUndergroundPrisonKey_Request
{
	public static NShared.RechargeUndergroundPrisonKey.Request Parse(string s)
	{
		return Parse(s, new NShared.RechargeUndergroundPrisonKey.Request());
	}

	public static NShared.RechargeUndergroundPrisonKey.Request Parse(string s, NShared.RechargeUndergroundPrisonKey.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RechargeUndergroundPrisonKey.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RechargeUndergroundPrisonKey.Request());
	}

	public static NShared.RechargeUndergroundPrisonKey.Request Parse(IDictionary d, NShared.RechargeUndergroundPrisonKey.Request o)
	{
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RechargeUndergroundPrisonKey.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RechargeUndergroundPrisonKey.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
