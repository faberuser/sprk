using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildSuppressMemberScoreInfo
{
	public static GuildSuppressMemberScoreInfo Parse(string s)
	{
		return Parse(s, new GuildSuppressMemberScoreInfo());
	}

	public static GuildSuppressMemberScoreInfo Parse(string s, GuildSuppressMemberScoreInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildSuppressMemberScoreInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildSuppressMemberScoreInfo());
	}

	public static GuildSuppressMemberScoreInfo Parse(IDictionary d, GuildSuppressMemberScoreInfo o)
	{
		o.GuildId = BaseJsonMarshaler.ParseValueLong(d, "GuildId");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.NickName = BaseJsonMarshaler.ParseValueString(d, "NickName");
		o.AvatarHeroIndex = BaseJsonMarshaler.ParseValueInt(d, "AvatarHeroIndex");
		o.Score = BaseJsonMarshaler.ParseValueLong(d, "Score");
		return o;
	}

	public static string EncodeJsonString(GuildSuppressMemberScoreInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildSuppressMemberScoreInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GuildId", o.GuildId.ToString());
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		if (o.NickName != null)
		{
			dictionary.Add("NickName", o.NickName);
		}
		dictionary.Add("AvatarHeroIndex", o.AvatarHeroIndex.ToString());
		dictionary.Add("Score", o.Score.ToString());
		return dictionary;
	}
}
