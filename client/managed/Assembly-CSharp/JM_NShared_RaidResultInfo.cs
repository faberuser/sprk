using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_RaidResultInfo
{
	public static RaidResultInfo Parse(string s)
	{
		return Parse(s, new RaidResultInfo());
	}

	public static RaidResultInfo Parse(string s, RaidResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RaidResultInfo Parse(IDictionary d)
	{
		return Parse(d, new RaidResultInfo());
	}

	public static RaidResultInfo Parse(IDictionary d, RaidResultInfo o)
	{
		o.LastRaidResetTime = BaseJsonMarshaler.ParseValueString(d, "LastRaidResetTime");
		o.RaidInfos = BaseJsonMarshaler.ParseArray(d, "RaidInfos", JM_NShared_RaidInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(RaidResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RaidResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.LastRaidResetTime != null)
		{
			dictionary.Add("LastRaidResetTime", o.LastRaidResetTime);
		}
		dictionary.Add("RaidInfos", BaseJsonMarshaler.EncodeArray(o.RaidInfos, JM_NShared_RaidInfo.EncodeJsonObject));
		return dictionary;
	}
}
