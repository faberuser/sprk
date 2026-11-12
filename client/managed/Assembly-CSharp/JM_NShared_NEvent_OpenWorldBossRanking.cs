using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_OpenWorldBossRanking
{
	public static OpenWorldBossRanking Parse(string s)
	{
		return Parse(s, new OpenWorldBossRanking());
	}

	public static OpenWorldBossRanking Parse(string s, OpenWorldBossRanking o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OpenWorldBossRanking Parse(IDictionary d)
	{
		return Parse(d, new OpenWorldBossRanking());
	}

	public static OpenWorldBossRanking Parse(IDictionary d, OpenWorldBossRanking o)
	{
		o.IsActive = BaseJsonMarshaler.ParseValueBool(d, "IsActive");
		return o;
	}

	public static string EncodeJsonString(OpenWorldBossRanking o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OpenWorldBossRanking o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("IsActive", o.IsActive.ToString());
		return dictionary;
	}
}
