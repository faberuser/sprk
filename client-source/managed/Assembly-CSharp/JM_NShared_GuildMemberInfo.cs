using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildMemberInfo
{
	public static GuildMemberInfo Parse(string s)
	{
		return Parse(s, new GuildMemberInfo());
	}

	public static GuildMemberInfo Parse(string s, GuildMemberInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildMemberInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildMemberInfo());
	}

	public static GuildMemberInfo Parse(IDictionary d, GuildMemberInfo o)
	{
		o.Rank = BaseJsonMarshaler.ParseValueByte(d, "Rank");
		o.AccActivityPoint = BaseJsonMarshaler.ParseValueInt(d, "AccActivityPoint");
		o.AccSuppressPoint = BaseJsonMarshaler.ParseValueInt(d, "AccSuppressPoint");
		o.AccWood = BaseJsonMarshaler.ParseValueInt(d, "AccWood");
		o.AccStone = BaseJsonMarshaler.ParseValueInt(d, "AccStone");
		o.AccMetal = BaseJsonMarshaler.ParseValueInt(d, "AccMetal");
		o.RaidEnterCount = BaseJsonMarshaler.ParseValueByte(d, "RaidEnterCount");
		o.ContributeCount = BaseJsonMarshaler.ParseValueByte(d, "ContributeCount");
		o.ReqGuildSkill = BaseJsonMarshaler.ParseValueByte(d, "ReqGuildSkill");
		o.BidRaidIndex = BaseJsonMarshaler.ParseValueInt(d, "BidRaidIndex");
		o.BidRaidRewardItemIndex = BaseJsonMarshaler.ParseValueInt(d, "BidRaidRewardItemIndex");
		o.BidRaidRewardTime = BaseJsonMarshaler.ParseValueString(d, "BidRaidRewardTime");
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.TeamLevel = BaseJsonMarshaler.ParseValueInt(d, "TeamLevel");
		o.Nick = BaseJsonMarshaler.ParseValueString(d, "Nick");
		o.VIPLevel = BaseJsonMarshaler.ParseValueInt(d, "VIPLevel");
		o.AvatarHeroIndex = BaseJsonMarshaler.ParseValueInt(d, "AvatarHeroIndex");
		o.LastLoginTime = BaseJsonMarshaler.ParseValueString(d, "LastLoginTime");
		o.LastLogoutTime = BaseJsonMarshaler.ParseValueString(d, "LastLogoutTime");
		o.TierIndex = BaseJsonMarshaler.ParseValueInt(d, "TierIndex");
		o.MatchScore = BaseJsonMarshaler.ParseValueInt(d, "MatchScore");
		o.SeasonWin = BaseJsonMarshaler.ParseValueInt(d, "SeasonWin");
		o.SeasonLose = BaseJsonMarshaler.ParseValueInt(d, "SeasonLose");
		o.SuccessiveWin = BaseJsonMarshaler.ParseValueInt(d, "SuccessiveWin");
		o.IsOnline = BaseJsonMarshaler.ParseValueBool(d, "IsOnline");
		o.LastChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "LastChapterIndex");
		o.LastDungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "LastDungeonIndex");
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.CostumeIndex = BaseJsonMarshaler.ParseValueInt(d, "CostumeIndex");
		o.HideUniqueWeapon = BaseJsonMarshaler.ParseValueByte(d, "HideUniqueWeapon");
		o.WeaponCostumeIndex = BaseJsonMarshaler.ParseValueInt(d, "WeaponCostumeIndex");
		o.HairCostumeIndex = BaseJsonMarshaler.ParseValueInt(d, "HairCostumeIndex");
		o.AccessoryCostumeIndex1 = BaseJsonMarshaler.ParseValueInt(d, "AccessoryCostumeIndex1");
		o.PositionInfo1 = BaseJsonMarshaler.ParseValueString(d, "PositionInfo1");
		o.AccessoryCostumeIndex2 = BaseJsonMarshaler.ParseValueInt(d, "AccessoryCostumeIndex2");
		o.PositionInfo2 = BaseJsonMarshaler.ParseValueString(d, "PositionInfo2");
		o.AccessoryCostumeIndex3 = BaseJsonMarshaler.ParseValueInt(d, "AccessoryCostumeIndex3");
		o.PositionInfo3 = BaseJsonMarshaler.ParseValueString(d, "PositionInfo3");
		o.AccessoryCostumeIndex4 = BaseJsonMarshaler.ParseValueInt(d, "AccessoryCostumeIndex4");
		o.PositionInfo4 = BaseJsonMarshaler.ParseValueString(d, "PositionInfo4");
		o.AccessoryCostumeIndex5 = BaseJsonMarshaler.ParseValueInt(d, "AccessoryCostumeIndex5");
		o.PositionInfo5 = BaseJsonMarshaler.ParseValueString(d, "PositionInfo5");
		o.AccessoryCostumeIndex6 = BaseJsonMarshaler.ParseValueInt(d, "AccessoryCostumeIndex6");
		o.PositionInfo6 = BaseJsonMarshaler.ParseValueString(d, "PositionInfo6");
		SprkAccessories.Read(o, d);
		return o;
	}

	public static string EncodeJsonString(GuildMemberInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildMemberInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			SprkAccessories.Write(dictionary, o);
			return dictionary;
		}
		dictionary.Add("Rank", o.Rank.ToString());
		dictionary.Add("AccActivityPoint", o.AccActivityPoint.ToString());
		dictionary.Add("AccSuppressPoint", o.AccSuppressPoint.ToString());
		dictionary.Add("AccWood", o.AccWood.ToString());
		dictionary.Add("AccStone", o.AccStone.ToString());
		dictionary.Add("AccMetal", o.AccMetal.ToString());
		dictionary.Add("RaidEnterCount", o.RaidEnterCount.ToString());
		dictionary.Add("ContributeCount", o.ContributeCount.ToString());
		dictionary.Add("ReqGuildSkill", o.ReqGuildSkill.ToString());
		dictionary.Add("BidRaidIndex", o.BidRaidIndex.ToString());
		dictionary.Add("BidRaidRewardItemIndex", o.BidRaidRewardItemIndex.ToString());
		if (o.BidRaidRewardTime != null)
		{
			dictionary.Add("BidRaidRewardTime", o.BidRaidRewardTime);
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("TeamLevel", o.TeamLevel.ToString());
		if (o.Nick != null)
		{
			dictionary.Add("Nick", o.Nick);
		}
		dictionary.Add("VIPLevel", o.VIPLevel.ToString());
		dictionary.Add("AvatarHeroIndex", o.AvatarHeroIndex.ToString());
		if (o.LastLoginTime != null)
		{
			dictionary.Add("LastLoginTime", o.LastLoginTime);
		}
		if (o.LastLogoutTime != null)
		{
			dictionary.Add("LastLogoutTime", o.LastLogoutTime);
		}
		dictionary.Add("TierIndex", o.TierIndex.ToString());
		dictionary.Add("MatchScore", o.MatchScore.ToString());
		dictionary.Add("SeasonWin", o.SeasonWin.ToString());
		dictionary.Add("SeasonLose", o.SeasonLose.ToString());
		dictionary.Add("SuccessiveWin", o.SuccessiveWin.ToString());
		dictionary.Add("IsOnline", o.IsOnline.ToString());
		dictionary.Add("LastChapterIndex", o.LastChapterIndex.ToString());
		dictionary.Add("LastDungeonIndex", o.LastDungeonIndex.ToString());
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("CostumeIndex", o.CostumeIndex.ToString());
		dictionary.Add("HideUniqueWeapon", o.HideUniqueWeapon.ToString());
		dictionary.Add("WeaponCostumeIndex", o.WeaponCostumeIndex.ToString());
		dictionary.Add("HairCostumeIndex", o.HairCostumeIndex.ToString());
		dictionary.Add("AccessoryCostumeIndex1", o.AccessoryCostumeIndex1.ToString());
		if (o.PositionInfo1 != null)
		{
			dictionary.Add("PositionInfo1", o.PositionInfo1);
		}
		dictionary.Add("AccessoryCostumeIndex2", o.AccessoryCostumeIndex2.ToString());
		if (o.PositionInfo2 != null)
		{
			dictionary.Add("PositionInfo2", o.PositionInfo2);
		}
		dictionary.Add("AccessoryCostumeIndex3", o.AccessoryCostumeIndex3.ToString());
		if (o.PositionInfo3 != null)
		{
			dictionary.Add("PositionInfo3", o.PositionInfo3);
		}
		dictionary.Add("AccessoryCostumeIndex4", o.AccessoryCostumeIndex4.ToString());
		if (o.PositionInfo4 != null)
		{
			dictionary.Add("PositionInfo4", o.PositionInfo4);
		}
		dictionary.Add("AccessoryCostumeIndex5", o.AccessoryCostumeIndex5.ToString());
		if (o.PositionInfo5 != null)
		{
			dictionary.Add("PositionInfo5", o.PositionInfo5);
		}
		dictionary.Add("AccessoryCostumeIndex6", o.AccessoryCostumeIndex6.ToString());
		if (o.PositionInfo6 != null)
		{
			dictionary.Add("PositionInfo6", o.PositionInfo6);
		}
		SprkAccessories.Write(dictionary, o);
		return dictionary;
	}
}
