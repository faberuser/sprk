using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildSuppressPartyInfo
{
	public static GuildSuppressPartyInfo Parse(string s)
	{
		return Parse(s, new GuildSuppressPartyInfo());
	}

	public static GuildSuppressPartyInfo Parse(string s, GuildSuppressPartyInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildSuppressPartyInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildSuppressPartyInfo());
	}

	public static GuildSuppressPartyInfo Parse(IDictionary d, GuildSuppressPartyInfo o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.HeroCount = BaseJsonMarshaler.ParseValueLong(d, "HeroCount");
		o.HeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndices", BaseJsonMarshaler.CastToInt);
		return o;
	}

	public static string EncodeJsonString(GuildSuppressPartyInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildSuppressPartyInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("HeroCount", o.HeroCount.ToString());
		dictionary.Add("HeroIndices", BaseJsonMarshaler.EncodeArray(o.HeroIndices));
		return dictionary;
	}
}
