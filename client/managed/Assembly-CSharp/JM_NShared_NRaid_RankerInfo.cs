using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NRaid;

public static class JM_NShared_NRaid_RankerInfo
{
	public static RankerInfo Parse(string s)
	{
		return Parse(s, new RankerInfo());
	}

	public static RankerInfo Parse(string s, RankerInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RankerInfo Parse(IDictionary d)
	{
		return Parse(d, new RankerInfo());
	}

	public static RankerInfo Parse(IDictionary d, RankerInfo o)
	{
		o.Nick = BaseJsonMarshaler.ParseValueString(d, "Nick");
		o.AvatarHeroIndex = BaseJsonMarshaler.ParseValueInt(d, "AvatarHeroIndex");
		o.AccountId = BaseJsonMarshaler.ParseValueInt(d, "AccountId");
		return o;
	}

	public static string EncodeJsonString(RankerInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RankerInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Nick != null)
		{
			dictionary.Add("Nick", o.Nick);
		}
		dictionary.Add("AvatarHeroIndex", o.AvatarHeroIndex.ToString());
		dictionary.Add("AccountId", o.AccountId.ToString());
		return dictionary;
	}
}
