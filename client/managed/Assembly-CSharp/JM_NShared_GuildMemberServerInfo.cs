using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildMemberServerInfo
{
	public static GuildMemberServerInfo Parse(string s)
	{
		return Parse(s, new GuildMemberServerInfo());
	}

	public static GuildMemberServerInfo Parse(string s, GuildMemberServerInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildMemberServerInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildMemberServerInfo());
	}

	public static GuildMemberServerInfo Parse(IDictionary d, GuildMemberServerInfo o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		o.Rank = BaseJsonMarshaler.ParseValueByte(d, "Rank");
		o.Nick = BaseJsonMarshaler.ParseValueString(d, "Nick");
		o.TeamLevel = BaseJsonMarshaler.ParseValueInt(d, "TeamLevel");
		o.AvatarHeroIndex = BaseJsonMarshaler.ParseValueInt(d, "AvatarHeroIndex");
		o.CountryCode = BaseJsonMarshaler.ParseValueString(d, "CountryCode");
		return o;
	}

	public static string EncodeJsonString(GuildMemberServerInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildMemberServerInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		dictionary.Add("Rank", o.Rank.ToString());
		if (o.Nick != null)
		{
			dictionary.Add("Nick", o.Nick);
		}
		dictionary.Add("TeamLevel", o.TeamLevel.ToString());
		dictionary.Add("AvatarHeroIndex", o.AvatarHeroIndex.ToString());
		if (o.CountryCode != null)
		{
			dictionary.Add("CountryCode", o.CountryCode);
		}
		return dictionary;
	}
}
