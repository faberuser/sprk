using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_SeasonChanged
{
	public static SeasonChanged Parse(string s)
	{
		return Parse(s, new SeasonChanged());
	}

	public static SeasonChanged Parse(string s, SeasonChanged o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static SeasonChanged Parse(IDictionary d)
	{
		return Parse(d, new SeasonChanged());
	}

	public static SeasonChanged Parse(IDictionary d, SeasonChanged o)
	{
		o.SeasonData = BaseJsonMarshaler.ParseObject(d, "SeasonData", JM_NShared_MatchSeasonData.Parse);
		return o;
	}

	public static string EncodeJsonString(SeasonChanged o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(SeasonChanged o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SeasonData", JM_NShared_MatchSeasonData.EncodeJsonObject(o.SeasonData));
		return dictionary;
	}
}
