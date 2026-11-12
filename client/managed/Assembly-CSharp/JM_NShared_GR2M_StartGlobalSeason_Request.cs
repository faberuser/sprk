using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GR2M_StartGlobalSeason;

public static class JM_NShared_GR2M_StartGlobalSeason_Request
{
	public static NShared.GR2M_StartGlobalSeason.Request Parse(string s)
	{
		return Parse(s, new NShared.GR2M_StartGlobalSeason.Request());
	}

	public static NShared.GR2M_StartGlobalSeason.Request Parse(string s, NShared.GR2M_StartGlobalSeason.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GR2M_StartGlobalSeason.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GR2M_StartGlobalSeason.Request());
	}

	public static NShared.GR2M_StartGlobalSeason.Request Parse(IDictionary d, NShared.GR2M_StartGlobalSeason.Request o)
	{
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		return o;
	}

	public static string EncodeJsonString(NShared.GR2M_StartGlobalSeason.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GR2M_StartGlobalSeason.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		return dictionary;
	}
}
