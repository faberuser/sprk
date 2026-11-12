using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildRaidChapterRewardData
{
	public static GuildRaidChapterRewardData Parse(string s)
	{
		return Parse(s, new GuildRaidChapterRewardData());
	}

	public static GuildRaidChapterRewardData Parse(string s, GuildRaidChapterRewardData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildRaidChapterRewardData Parse(IDictionary d)
	{
		return Parse(d, new GuildRaidChapterRewardData());
	}

	public static GuildRaidChapterRewardData Parse(IDictionary d, GuildRaidChapterRewardData o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.Difficulty = BaseJsonMarshaler.ParseValueInt(d, "Difficulty");
		o.TotalDamage = BaseJsonMarshaler.ParseValueLong(d, "TotalDamage");
		o.Credit = BaseJsonMarshaler.ParseValueString(d, "Credit");
		o.RewardIndex = BaseJsonMarshaler.ParseValueInt(d, "RewardIndex");
		o.MailTitleKey = BaseJsonMarshaler.ParseValueString(d, "MailTitleKey");
		o.MailContentKey = BaseJsonMarshaler.ParseValueString(d, "MailContentKey");
		return o;
	}

	public static string EncodeJsonString(GuildRaidChapterRewardData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildRaidChapterRewardData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("Difficulty", o.Difficulty.ToString());
		dictionary.Add("TotalDamage", o.TotalDamage.ToString());
		if (o.Credit != null)
		{
			dictionary.Add("Credit", o.Credit);
		}
		dictionary.Add("RewardIndex", o.RewardIndex.ToString());
		if (o.MailTitleKey != null)
		{
			dictionary.Add("MailTitleKey", o.MailTitleKey);
		}
		if (o.MailContentKey != null)
		{
			dictionary.Add("MailContentKey", o.MailContentKey);
		}
		return dictionary;
	}
}
