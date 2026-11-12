using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildArenaMemberInfo
{
	public static GuildArenaMemberInfo Parse(string s)
	{
		return Parse(s, new GuildArenaMemberInfo());
	}

	public static GuildArenaMemberInfo Parse(string s, GuildArenaMemberInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildArenaMemberInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildArenaMemberInfo());
	}

	public static GuildArenaMemberInfo Parse(IDictionary d, GuildArenaMemberInfo o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.SessionIndex = BaseJsonMarshaler.ParseValueInt(d, "SessionIndex");
		o.BattledHeroIndices = BaseJsonMarshaler.ParseValueString(d, "BattledHeroIndices");
		return o;
	}

	public static string EncodeJsonString(GuildArenaMemberInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildArenaMemberInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("SessionIndex", o.SessionIndex.ToString());
		if (o.BattledHeroIndices != null)
		{
			dictionary.Add("BattledHeroIndices", o.BattledHeroIndices);
		}
		return dictionary;
	}
}
