using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildSuppressDungeonData
{
	public static GuildSuppressDungeonData Parse(string s)
	{
		return Parse(s, new GuildSuppressDungeonData());
	}

	public static GuildSuppressDungeonData Parse(string s, GuildSuppressDungeonData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildSuppressDungeonData Parse(IDictionary d)
	{
		return Parse(d, new GuildSuppressDungeonData());
	}

	public static GuildSuppressDungeonData Parse(IDictionary d, GuildSuppressDungeonData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.DefaultRewardIndex = BaseJsonMarshaler.ParseValueInt(d, "DefaultRewardIndex");
		o.KillRewardIndex = BaseJsonMarshaler.ParseValueInt(d, "KillRewardIndex");
		o.KillActivityPoint = BaseJsonMarshaler.ParseValueInt(d, "KillActivityPoint");
		o.BuyableRewardIndex = BaseJsonMarshaler.ParseValueInt(d, "BuyableRewardIndex");
		o.Tactic = BaseJsonMarshaler.ParseValueString(d, "Tactic");
		o.RecommendedHeroIndex = BaseJsonMarshaler.ParseArrayCast(d, "RecommendedHeroIndex", BaseJsonMarshaler.CastToInt);
		o.BanIndex = BaseJsonMarshaler.ParseValueInt(d, "BanIndex");
		o.PortraitPath = BaseJsonMarshaler.ParseValueString(d, "PortraitPath");
		o.PreviewBg = BaseJsonMarshaler.ParseValueString(d, "PreviewBg");
		o.FinishModeRatio = BaseJsonMarshaler.ParseValueLong(d, "FinishModeRatio");
		o.ShowInfoSkill = BaseJsonMarshaler.ParseArrayCast(d, "ShowInfoSkill", BaseJsonMarshaler.CastToInt);
		o.IconPath = BaseJsonMarshaler.ParseValueString(d, "IconPath");
		o.StatusBoardPortraitPath = BaseJsonMarshaler.ParseValueString(d, "StatusBoardPortraitPath");
		return o;
	}

	public static string EncodeJsonString(GuildSuppressDungeonData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildSuppressDungeonData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("DefaultRewardIndex", o.DefaultRewardIndex.ToString());
		dictionary.Add("KillRewardIndex", o.KillRewardIndex.ToString());
		dictionary.Add("KillActivityPoint", o.KillActivityPoint.ToString());
		dictionary.Add("BuyableRewardIndex", o.BuyableRewardIndex.ToString());
		if (o.Tactic != null)
		{
			dictionary.Add("Tactic", o.Tactic);
		}
		dictionary.Add("RecommendedHeroIndex", BaseJsonMarshaler.EncodeArray(o.RecommendedHeroIndex));
		dictionary.Add("BanIndex", o.BanIndex.ToString());
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
		if (o.IconPath != null)
		{
			dictionary.Add("IconPath", o.IconPath);
		}
		if (o.StatusBoardPortraitPath != null)
		{
			dictionary.Add("StatusBoardPortraitPath", o.StatusBoardPortraitPath);
		}
		return dictionary;
	}
}
