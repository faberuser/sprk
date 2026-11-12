using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.G2R_GetGlobalWorldBossRankerPages;

public static class JM_NShared_G2R_GetGlobalWorldBossRankerPages_Request
{
	public static NShared.G2R_GetGlobalWorldBossRankerPages.Request Parse(string s)
	{
		return Parse(s, new NShared.G2R_GetGlobalWorldBossRankerPages.Request());
	}

	public static NShared.G2R_GetGlobalWorldBossRankerPages.Request Parse(string s, NShared.G2R_GetGlobalWorldBossRankerPages.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_GetGlobalWorldBossRankerPages.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_GetGlobalWorldBossRankerPages.Request());
	}

	public static NShared.G2R_GetGlobalWorldBossRankerPages.Request Parse(IDictionary d, NShared.G2R_GetGlobalWorldBossRankerPages.Request o)
	{
		o.WorldBossIndex = BaseJsonMarshaler.ParseValueInt(d, "WorldBossIndex");
		o.Season = BaseJsonMarshaler.ParseValueInt(d, "Season");
		o.StartPage = BaseJsonMarshaler.ParseValueInt(d, "StartPage");
		o.EndPage = BaseJsonMarshaler.ParseValueInt(d, "EndPage");
		return o;
	}

	public static string EncodeJsonString(NShared.G2R_GetGlobalWorldBossRankerPages.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.G2R_GetGlobalWorldBossRankerPages.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("WorldBossIndex", o.WorldBossIndex.ToString());
		dictionary.Add("Season", o.Season.ToString());
		dictionary.Add("StartPage", o.StartPage.ToString());
		dictionary.Add("EndPage", o.EndPage.ToString());
		return dictionary;
	}
}
