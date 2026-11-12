using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildRaidDummyDungeonData
{
	public static GuildRaidDummyDungeonData Parse(string s)
	{
		return Parse(s, new GuildRaidDummyDungeonData());
	}

	public static GuildRaidDummyDungeonData Parse(string s, GuildRaidDummyDungeonData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildRaidDummyDungeonData Parse(IDictionary d)
	{
		return Parse(d, new GuildRaidDummyDungeonData());
	}

	public static GuildRaidDummyDungeonData Parse(IDictionary d, GuildRaidDummyDungeonData o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.Step = BaseJsonMarshaler.ParseValueInt(d, "Step");
		o.BonusDungeonRate = BaseJsonMarshaler.ParseValueInt(d, "BonusDungeonRate");
		o.KillRewardIndex = BaseJsonMarshaler.ParseValueInt(d, "KillRewardIndex");
		o.KillActivityPoint = BaseJsonMarshaler.ParseValueInt(d, "KillActivityPoint");
		o.BuyableRewardIndex = BaseJsonMarshaler.ParseValueInt(d, "BuyableRewardIndex");
		o.Tactic = BaseJsonMarshaler.ParseValueString(d, "Tactic");
		o.RecommendedHeroIndex = BaseJsonMarshaler.ParseArrayCast(d, "RecommendedHeroIndex", BaseJsonMarshaler.CastToInt);
		o.PortraitPath = BaseJsonMarshaler.ParseValueString(d, "PortraitPath");
		o.PreviewBg = BaseJsonMarshaler.ParseValueString(d, "PreviewBg");
		o.FinishModeRatio = BaseJsonMarshaler.ParseValueLong(d, "FinishModeRatio");
		o.ShowInfoSkill = BaseJsonMarshaler.ParseArrayCast(d, "ShowInfoSkill", BaseJsonMarshaler.CastToInt);
		return o;
	}

	public static string EncodeJsonString(GuildRaidDummyDungeonData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildRaidDummyDungeonData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("Step", o.Step.ToString());
		dictionary.Add("BonusDungeonRate", o.BonusDungeonRate.ToString());
		dictionary.Add("KillRewardIndex", o.KillRewardIndex.ToString());
		dictionary.Add("KillActivityPoint", o.KillActivityPoint.ToString());
		dictionary.Add("BuyableRewardIndex", o.BuyableRewardIndex.ToString());
		if (o.Tactic != null)
		{
			dictionary.Add("Tactic", o.Tactic);
		}
		dictionary.Add("RecommendedHeroIndex", BaseJsonMarshaler.EncodeArray(o.RecommendedHeroIndex));
		if (o.PortraitPath != null)
		{
			dictionary.Add("PortraitPath", o.PortraitPath);
		}
		if (o.PreviewBg != null)
		{
			dictionary.Add("PreviewBg", o.PreviewBg);
		}
		dictionary.Add("FinishModeRatio", o.FinishModeRatio.ToString());
		dictionary.Add("ShowInfoSkill", BaseJsonMarshaler.EncodeArray(o.ShowInfoSkill));
		return dictionary;
	}
}
