using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_HeroInfo
{
	public static HeroInfo Parse(string s)
	{
		return Parse(s, new HeroInfo());
	}

	public static HeroInfo Parse(string s, HeroInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static HeroInfo Parse(IDictionary d)
	{
		return Parse(d, new HeroInfo());
	}

	public static HeroInfo Parse(IDictionary d, HeroInfo o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.Level = BaseJsonMarshaler.ParseValueShort(d, "Level");
		o.Exp = BaseJsonMarshaler.ParseValueInt(d, "Exp");
		o.Star = BaseJsonMarshaler.ParseValueSByte(d, "Star");
		o.CreatedTime = BaseJsonMarshaler.ParseValueString(d, "CreatedTime");
		o.SkillLevel1 = BaseJsonMarshaler.ParseValueShort(d, "SkillLevel1");
		o.SkillLevel2 = BaseJsonMarshaler.ParseValueShort(d, "SkillLevel2");
		o.SkillLevel3 = BaseJsonMarshaler.ParseValueShort(d, "SkillLevel3");
		o.SkillLevel4 = BaseJsonMarshaler.ParseValueShort(d, "SkillLevel4");
		o.SkillLevel5 = BaseJsonMarshaler.ParseValueShort(d, "SkillLevel5");
		o.EquipItemSlotIndex1 = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex1");
		o.EquipItemSlotIndex2 = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex2");
		o.EquipItemSlotIndex3 = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex3");
		o.EquipItemSlotIndex4 = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex4");
		o.EquipItemSlotIndex5 = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex5");
		o.EquipItemSlotIndex6 = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex6");
		o.EquipItemSlotIndex7 = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex7");
		o.EquipItemSlotIndex8 = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex8");
		o.EquipItemSlotIndex9 = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex9");
		o.EquipItemSlotIndex10 = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex10");
		o.SkillExtend1 = BaseJsonMarshaler.ParseValueShort(d, "SkillExtend1");
		o.SkillExtend2 = BaseJsonMarshaler.ParseValueShort(d, "SkillExtend2");
		o.SkillExtend3 = BaseJsonMarshaler.ParseValueShort(d, "SkillExtend3");
		o.SkillExtend4 = BaseJsonMarshaler.ParseValueShort(d, "SkillExtend4");
		o.SkillExtend5 = BaseJsonMarshaler.ParseValueShort(d, "SkillExtend5");
		o.EnterDungeon = BaseJsonMarshaler.ParseValueSByte(d, "EnterDungeon");
		o.Purified = BaseJsonMarshaler.ParseValueShort(d, "Purified");
		o.Transcended = BaseJsonMarshaler.ParseValueSByte(d, "Transcended");
		o.LimitBreakLevel = BaseJsonMarshaler.ParseValueSByte(d, "LimitBreakLevel");
		o.LimitBreakExp = BaseJsonMarshaler.ParseValueLong(d, "LimitBreakExp");
		o.FlaskItemIndex = BaseJsonMarshaler.ParseValueInt(d, "FlaskItemIndex");
		o.FlaskExp = BaseJsonMarshaler.ParseValueInt(d, "FlaskExp");
		o.CostumeIndex = BaseJsonMarshaler.ParseValueInt(d, "CostumeIndex");
		o.WeaponCostumeIndex = BaseJsonMarshaler.ParseValueInt(d, "WeaponCostumeIndex");
		o.HairCostumeIndex = BaseJsonMarshaler.ParseValueInt(d, "HairCostumeIndex");
		o.HideUniqueWeapon = BaseJsonMarshaler.ParseValueByte(d, "HideUniqueWeapon");
		o.TranscendPoint = BaseJsonMarshaler.ParseValueShort(d, "TranscendPoint");
		o.TranscendSkillPage1 = BaseJsonMarshaler.ParseValueString(d, "TranscendSkillPage1");
		o.TranscendSkillPage2 = BaseJsonMarshaler.ParseValueString(d, "TranscendSkillPage2");
		o.TranscendSkillPage3 = BaseJsonMarshaler.ParseValueString(d, "TranscendSkillPage3");
		o.TranscendSkillPage4 = BaseJsonMarshaler.ParseValueString(d, "TranscendSkillPage4");
		o.TranscendSkillPage5 = BaseJsonMarshaler.ParseValueString(d, "TranscendSkillPage5");
		o.TranscendSkillPage6 = BaseJsonMarshaler.ParseValueString(d, "TranscendSkillPage6");
		o.ApplySkillPage = BaseJsonMarshaler.ParseValueInt(d, "ApplySkillPage");
		o.ClosenessPoint = BaseJsonMarshaler.ParseValueInt(d, "ClosenessPoint");
		o.LastGreetingTime = BaseJsonMarshaler.ParseValueString(d, "LastGreetingTime");
		o.LastConversationTime = BaseJsonMarshaler.ParseValueString(d, "LastConversationTime");
		o.LastGiftTime = BaseJsonMarshaler.ParseValueString(d, "LastGiftTime");
		o.MaxRewardedTime = BaseJsonMarshaler.ParseValueString(d, "MaxRewardedTime");
		o.LastRouletteTime = BaseJsonMarshaler.ParseValueString(d, "LastRouletteTime");
		o.AccessoryCostumeIndex1 = BaseJsonMarshaler.ParseValueInt(d, "AccessoryCostumeIndex1");
		o.AccessoryCostumeIndex2 = BaseJsonMarshaler.ParseValueInt(d, "AccessoryCostumeIndex2");
		o.AccessoryCostumeIndex3 = BaseJsonMarshaler.ParseValueInt(d, "AccessoryCostumeIndex3");
		o.AccessoryCostumeIndex4 = BaseJsonMarshaler.ParseValueInt(d, "AccessoryCostumeIndex4");
		o.AccessoryCostumeIndex5 = BaseJsonMarshaler.ParseValueInt(d, "AccessoryCostumeIndex5");
		o.AccessoryCostumeIndex6 = BaseJsonMarshaler.ParseValueInt(d, "AccessoryCostumeIndex6");
		o.ApplyRunePage = BaseJsonMarshaler.ParseValueInt(d, "ApplyRunePage");
		o.HeroRunePageInfos = BaseJsonMarshaler.ParseArray(d, "HeroRunePageInfos", JM_NShared_HeroRunePageInfo.Parse);
		SprkAccessories.Read(o, d);
		return o;
	}

	public static string EncodeJsonString(HeroInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(HeroInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			SprkAccessories.Write(dictionary, o);
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("Level", o.Level.ToString());
		dictionary.Add("Exp", o.Exp.ToString());
		dictionary.Add("Star", o.Star.ToString());
		if (o.CreatedTime != null)
		{
			dictionary.Add("CreatedTime", o.CreatedTime);
		}
		dictionary.Add("SkillLevel1", o.SkillLevel1.ToString());
		dictionary.Add("SkillLevel2", o.SkillLevel2.ToString());
		dictionary.Add("SkillLevel3", o.SkillLevel3.ToString());
		dictionary.Add("SkillLevel4", o.SkillLevel4.ToString());
		dictionary.Add("SkillLevel5", o.SkillLevel5.ToString());
		dictionary.Add("EquipItemSlotIndex1", o.EquipItemSlotIndex1.ToString());
		dictionary.Add("EquipItemSlotIndex2", o.EquipItemSlotIndex2.ToString());
		dictionary.Add("EquipItemSlotIndex3", o.EquipItemSlotIndex3.ToString());
		dictionary.Add("EquipItemSlotIndex4", o.EquipItemSlotIndex4.ToString());
		dictionary.Add("EquipItemSlotIndex5", o.EquipItemSlotIndex5.ToString());
		dictionary.Add("EquipItemSlotIndex6", o.EquipItemSlotIndex6.ToString());
		dictionary.Add("EquipItemSlotIndex7", o.EquipItemSlotIndex7.ToString());
		dictionary.Add("EquipItemSlotIndex8", o.EquipItemSlotIndex8.ToString());
		dictionary.Add("EquipItemSlotIndex9", o.EquipItemSlotIndex9.ToString());
		dictionary.Add("EquipItemSlotIndex10", o.EquipItemSlotIndex10.ToString());
		dictionary.Add("SkillExtend1", o.SkillExtend1.ToString());
		dictionary.Add("SkillExtend2", o.SkillExtend2.ToString());
		dictionary.Add("SkillExtend3", o.SkillExtend3.ToString());
		dictionary.Add("SkillExtend4", o.SkillExtend4.ToString());
		dictionary.Add("SkillExtend5", o.SkillExtend5.ToString());
		dictionary.Add("EnterDungeon", o.EnterDungeon.ToString());
		dictionary.Add("Purified", o.Purified.ToString());
		dictionary.Add("Transcended", o.Transcended.ToString());
		dictionary.Add("LimitBreakLevel", o.LimitBreakLevel.ToString());
		dictionary.Add("LimitBreakExp", o.LimitBreakExp.ToString());
		dictionary.Add("FlaskItemIndex", o.FlaskItemIndex.ToString());
		dictionary.Add("FlaskExp", o.FlaskExp.ToString());
		dictionary.Add("CostumeIndex", o.CostumeIndex.ToString());
		dictionary.Add("WeaponCostumeIndex", o.WeaponCostumeIndex.ToString());
		dictionary.Add("HairCostumeIndex", o.HairCostumeIndex.ToString());
		dictionary.Add("HideUniqueWeapon", o.HideUniqueWeapon.ToString());
		dictionary.Add("TranscendPoint", o.TranscendPoint.ToString());
		if (o.TranscendSkillPage1 != null)
		{
			dictionary.Add("TranscendSkillPage1", o.TranscendSkillPage1);
		}
		if (o.TranscendSkillPage2 != null)
		{
			dictionary.Add("TranscendSkillPage2", o.TranscendSkillPage2);
		}
		if (o.TranscendSkillPage3 != null)
		{
			dictionary.Add("TranscendSkillPage3", o.TranscendSkillPage3);
		}
		if (o.TranscendSkillPage4 != null)
		{
			dictionary.Add("TranscendSkillPage4", o.TranscendSkillPage4);
		}
		if (o.TranscendSkillPage5 != null)
		{
			dictionary.Add("TranscendSkillPage5", o.TranscendSkillPage5);
		}
		if (o.TranscendSkillPage6 != null)
		{
			dictionary.Add("TranscendSkillPage6", o.TranscendSkillPage6);
		}
		dictionary.Add("ApplySkillPage", o.ApplySkillPage.ToString());
		dictionary.Add("ClosenessPoint", o.ClosenessPoint.ToString());
		if (o.LastGreetingTime != null)
		{
			dictionary.Add("LastGreetingTime", o.LastGreetingTime);
		}
		if (o.LastConversationTime != null)
		{
			dictionary.Add("LastConversationTime", o.LastConversationTime);
		}
		if (o.LastGiftTime != null)
		{
			dictionary.Add("LastGiftTime", o.LastGiftTime);
		}
		if (o.MaxRewardedTime != null)
		{
			dictionary.Add("MaxRewardedTime", o.MaxRewardedTime);
		}
		if (o.LastRouletteTime != null)
		{
			dictionary.Add("LastRouletteTime", o.LastRouletteTime);
		}
		dictionary.Add("AccessoryCostumeIndex1", o.AccessoryCostumeIndex1.ToString());
		dictionary.Add("AccessoryCostumeIndex2", o.AccessoryCostumeIndex2.ToString());
		dictionary.Add("AccessoryCostumeIndex3", o.AccessoryCostumeIndex3.ToString());
		dictionary.Add("AccessoryCostumeIndex4", o.AccessoryCostumeIndex4.ToString());
		dictionary.Add("AccessoryCostumeIndex5", o.AccessoryCostumeIndex5.ToString());
		dictionary.Add("AccessoryCostumeIndex6", o.AccessoryCostumeIndex6.ToString());
		dictionary.Add("ApplyRunePage", o.ApplyRunePage.ToString());
		dictionary.Add("HeroRunePageInfos", BaseJsonMarshaler.EncodeArray(o.HeroRunePageInfos, JM_NShared_HeroRunePageInfo.EncodeJsonObject));
		SprkAccessories.Write(dictionary, o);
		return dictionary;
	}
}
