using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildContributeRewardData
{
	public static GuildContributeRewardData Parse(string s)
	{
		return Parse(s, new GuildContributeRewardData());
	}

	public static GuildContributeRewardData Parse(string s, GuildContributeRewardData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildContributeRewardData Parse(IDictionary d)
	{
		return Parse(d, new GuildContributeRewardData());
	}

	public static GuildContributeRewardData Parse(IDictionary d, GuildContributeRewardData o)
	{
		o.ContributedGem = BaseJsonMarshaler.ParseValueInt(d, "ContributedGem");
		o.RewardGuildActivityPoint = BaseJsonMarshaler.ParseValueInt(d, "RewardGuildActivityPoint");
		return o;
	}

	public static string EncodeJsonString(GuildContributeRewardData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildContributeRewardData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ContributedGem", o.ContributedGem.ToString());
		dictionary.Add("RewardGuildActivityPoint", o.RewardGuildActivityPoint.ToString());
		return dictionary;
	}
}
