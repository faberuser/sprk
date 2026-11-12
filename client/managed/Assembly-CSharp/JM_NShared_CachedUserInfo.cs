using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CachedUserInfo
{
	public static CachedUserInfo Parse(string s)
	{
		return Parse(s, new CachedUserInfo());
	}

	public static CachedUserInfo Parse(string s, CachedUserInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CachedUserInfo Parse(IDictionary d)
	{
		return Parse(d, new CachedUserInfo());
	}

	public static CachedUserInfo Parse(IDictionary d, CachedUserInfo o)
	{
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

	public static string EncodeJsonString(CachedUserInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CachedUserInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			SprkAccessories.Write(dictionary, o);
			return dictionary;
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
