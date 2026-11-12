using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildJoinRequestPlayerInfo
{
	public static GuildJoinRequestPlayerInfo Parse(string s)
	{
		return Parse(s, new GuildJoinRequestPlayerInfo());
	}

	public static GuildJoinRequestPlayerInfo Parse(string s, GuildJoinRequestPlayerInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildJoinRequestPlayerInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildJoinRequestPlayerInfo());
	}

	public static GuildJoinRequestPlayerInfo Parse(IDictionary d, GuildJoinRequestPlayerInfo o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.TeamLevel = BaseJsonMarshaler.ParseValueByte(d, "TeamLevel");
		o.Nick = BaseJsonMarshaler.ParseValueString(d, "Nick");
		o.AvatarHeroIndex = BaseJsonMarshaler.ParseValueInt(d, "AvatarHeroIndex");
		o.LastLoginTime = BaseJsonMarshaler.ParseValueString(d, "LastLoginTime");
		return o;
	}

	public static string EncodeJsonString(GuildJoinRequestPlayerInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildJoinRequestPlayerInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("TeamLevel", o.TeamLevel.ToString());
		if (o.Nick != null)
		{
			dictionary.Add("Nick", o.Nick);
		}
		dictionary.Add("AvatarHeroIndex", o.AvatarHeroIndex.ToString());
		if (o.LastLoginTime != null)
		{
			dictionary.Add("LastLoginTime", o.LastLoginTime);
		}
		return dictionary;
	}
}
