using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CreatureData
{
	public static CreatureData Parse(string s)
	{
		return Parse(s, new CreatureData());
	}

	public static CreatureData Parse(string s, CreatureData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CreatureData Parse(IDictionary d)
	{
		return Parse(d, new CreatureData());
	}

	public static CreatureData Parse(IDictionary d, CreatureData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.ReplaceNameKey = BaseJsonMarshaler.ParseValueString(d, "ReplaceNameKey");
		o.NameParam = BaseJsonMarshaler.ParseArrayCast(d, "NameParam", BaseJsonMarshaler.CastToString);
		o.CodeName = BaseJsonMarshaler.ParseValueString(d, "CodeName");
		o.ModelCode = BaseJsonMarshaler.ParseValueString(d, "ModelCode");
		o.ModelPrefix = BaseJsonMarshaler.ParseValueString(d, "ModelPrefix");
		o.OpenType = BaseJsonMarshaler.ParseEnum<CreatureOpenType>(d, "OpenType");
		o.OpenStatus = BaseJsonMarshaler.ParseEnum<CreatureOpenStatusType>(d, "OpenStatus");
		o.OpenDate = BaseJsonMarshaler.ParseValueInt(d, "OpenDate");
		o.Race = BaseJsonMarshaler.ParseEnum<CreatureRaceType>(d, "Race");
		o.TagType = BaseJsonMarshaler.ParseEnum<CreatureTagType>(d, "TagType");
		o.AttrType = BaseJsonMarshaler.ParseArray<SkillAttrType>(d, "AttrType");
		o.WeakType = BaseJsonMarshaler.ParseArray<SkillAttrType>(d, "WeakType");
		o.ArchiveStory = BaseJsonMarshaler.ParseValueString(d, "ArchiveStory");
		o.ArchiveType = BaseJsonMarshaler.ParseEnum<ArchiveType>(d, "ArchiveType");
		o.AttackType = BaseJsonMarshaler.ParseEnum<AttackAttrType>(d, "AttackType");
		o.AttackWeakType = BaseJsonMarshaler.ParseArray<AttackAttrType>(d, "AttackWeakType");
		o.PositionType = BaseJsonMarshaler.ParseEnum<CreaturePositionType>(d, "PositionType");
		o.PositionWeight = BaseJsonMarshaler.ParseValueInt(d, "PositionWeight");
		o.SizeMm = BaseJsonMarshaler.ParseValueInt(d, "SizeMm");
		o.BlockDash = BaseJsonMarshaler.ParseValueBool(d, "BlockDash");
		o.HeightMm = BaseJsonMarshaler.ParseValueInt(d, "HeightMm");
		o.IgnoreCc = BaseJsonMarshaler.ParseArrayCast(d, "IgnoreCc", BaseJsonMarshaler.CastToString);
		o.TransformType = BaseJsonMarshaler.ParseEnum<CreatureTransformType>(d, "TransformType");
		o.AvoidSkillTargetType = BaseJsonMarshaler.ParseArray<SkillTargetType>(d, "AvoidSkillTargetType");
		o.OffsetMx = BaseJsonMarshaler.ParseValueInt(d, "OffsetMx");
		o.OffsetMz = BaseJsonMarshaler.ParseValueInt(d, "OffsetMz");
		o.GradeStoneItemCode = BaseJsonMarshaler.ParseValueString(d, "GradeStoneItemCode");
		o.StarStoneItemCode = BaseJsonMarshaler.ParseValueString(d, "StarStoneItemCode");
		o.DropItemWeight = BaseJsonMarshaler.ParseValueInt(d, "DropItemWeight");
		o.DropGoldWeight = BaseJsonMarshaler.ParseValueInt(d, "DropGoldWeight");
		o.BaseSkillIndex = BaseJsonMarshaler.ParseValueInt(d, "BaseSkillIndex");
		o.SkillIndex1 = BaseJsonMarshaler.ParseValueInt(d, "SkillIndex1");
		o.SkillIndex2 = BaseJsonMarshaler.ParseValueInt(d, "SkillIndex2");
		o.SkillIndex3 = BaseJsonMarshaler.ParseValueInt(d, "SkillIndex3");
		o.SkillIndex4 = BaseJsonMarshaler.ParseValueInt(d, "SkillIndex4");
		o.SkillIndex5 = BaseJsonMarshaler.ParseValueInt(d, "SkillIndex5");
		o.LeaderSkillIndex = BaseJsonMarshaler.ParseValueInt(d, "LeaderSkillIndex");
		o.SkillExtend1 = BaseJsonMarshaler.ParseArrayCast(d, "SkillExtend1", BaseJsonMarshaler.CastToInt);
		o.SkillExtend2 = BaseJsonMarshaler.ParseArrayCast(d, "SkillExtend2", BaseJsonMarshaler.CastToInt);
		o.SkillExtend3 = BaseJsonMarshaler.ParseArrayCast(d, "SkillExtend3", BaseJsonMarshaler.CastToInt);
		o.SkillExtend4 = BaseJsonMarshaler.ParseArrayCast(d, "SkillExtend4", BaseJsonMarshaler.CastToInt);
		o.SkillExtend5 = BaseJsonMarshaler.ParseArrayCast(d, "SkillExtend5", BaseJsonMarshaler.CastToInt);
		o.LinkedSkillIndex1 = BaseJsonMarshaler.ParseArrayCast(d, "LinkedSkillIndex1", BaseJsonMarshaler.CastToInt);
		o.LinkedSkillIndex2 = BaseJsonMarshaler.ParseArrayCast(d, "LinkedSkillIndex2", BaseJsonMarshaler.CastToInt);
		o.LinkedSkillIndex3 = BaseJsonMarshaler.ParseArrayCast(d, "LinkedSkillIndex3", BaseJsonMarshaler.CastToInt);
		o.LinkedSkillIndex4 = BaseJsonMarshaler.ParseArrayCast(d, "LinkedSkillIndex4", BaseJsonMarshaler.CastToInt);
		o.LinkedSkillIndex5 = BaseJsonMarshaler.ParseArrayCast(d, "LinkedSkillIndex5", BaseJsonMarshaler.CastToInt);
		o.ExtraSkill = BaseJsonMarshaler.ParseArrayCast(d, "ExtraSkill", BaseJsonMarshaler.CastToInt);
		o.ExtraActiveSkill = BaseJsonMarshaler.ParseArrayCast(d, "ExtraActiveSkill", BaseJsonMarshaler.CastToInt);
		o.ShowInfoSkill = BaseJsonMarshaler.ParseArrayCast(d, "ShowInfoSkill", BaseJsonMarshaler.CastToInt);
		o.GetTranscendSkillPoint = BaseJsonMarshaler.ParseArrayCast(d, "GetTranscendSkillPoint", BaseJsonMarshaler.CastToInt);
		o.TranscendSkill1 = BaseJsonMarshaler.ParseArrayCast(d, "TranscendSkill1", BaseJsonMarshaler.CastToInt);
		o.ReqSkillPoint1 = BaseJsonMarshaler.ParseArrayCast(d, "ReqSkillPoint1", BaseJsonMarshaler.CastToInt);
		o.TranscendSkill2 = BaseJsonMarshaler.ParseArrayCast(d, "TranscendSkill2", BaseJsonMarshaler.CastToInt);
		o.ReqSkillPoint2 = BaseJsonMarshaler.ParseArrayCast(d, "ReqSkillPoint2", BaseJsonMarshaler.CastToInt);
		o.TranscendSkill3 = BaseJsonMarshaler.ParseArrayCast(d, "TranscendSkill3", BaseJsonMarshaler.CastToInt);
		o.ReqSkillPoint3 = BaseJsonMarshaler.ParseArrayCast(d, "ReqSkillPoint3", BaseJsonMarshaler.CastToInt);
		o.TranscendSkill4 = BaseJsonMarshaler.ParseArrayCast(d, "TranscendSkill4", BaseJsonMarshaler.CastToInt);
		o.ReqSkillPoint4 = BaseJsonMarshaler.ParseArrayCast(d, "ReqSkillPoint4", BaseJsonMarshaler.CastToInt);
		o.TranscendSkill5 = BaseJsonMarshaler.ParseArrayCast(d, "TranscendSkill5", BaseJsonMarshaler.CastToInt);
		o.ReqSkillPoint5 = BaseJsonMarshaler.ParseArrayCast(d, "ReqSkillPoint5", BaseJsonMarshaler.CastToInt);
		o.StartHeroStar = BaseJsonMarshaler.ParseValueSByte(d, "StartHeroStar");
		o.StartHeroLevel = BaseJsonMarshaler.ParseValueInt(d, "StartHeroLevel");
		o.ReplaceDescKey = BaseJsonMarshaler.ParseValueString(d, "ReplaceDescKey");
		o.DescParam = BaseJsonMarshaler.ParseArrayCast(d, "DescParam", BaseJsonMarshaler.CastToString);
		o.ReplaceTitleKey = BaseJsonMarshaler.ParseValueString(d, "ReplaceTitleKey");
		o.TitleParam = BaseJsonMarshaler.ParseArrayCast(d, "TitleParam", BaseJsonMarshaler.CastToString);
		o.ClearTip = BaseJsonMarshaler.ParseValueString(d, "ClearTip");
		o.AddStamina = BaseJsonMarshaler.ParseValueInt(d, "AddStamina");
		o.IsDestroyOnDied = BaseJsonMarshaler.ParseValueBool(d, "IsDestroyOnDied");
		o.HeroPositionIcon1 = BaseJsonMarshaler.ParseValueString(d, "HeroPositionIcon1");
		o.HeroPositionText1 = BaseJsonMarshaler.ParseValueString(d, "HeroPositionText1");
		o.HeroPositionIcon2 = BaseJsonMarshaler.ParseValueString(d, "HeroPositionIcon2");
		o.HeroPositionText2 = BaseJsonMarshaler.ParseValueString(d, "HeroPositionText2");
		o.HeroPositionIcon3 = BaseJsonMarshaler.ParseValueString(d, "HeroPositionIcon3");
		o.HeroPositionText3 = BaseJsonMarshaler.ParseValueString(d, "HeroPositionText3");
		o.HeroPositionIcon4 = BaseJsonMarshaler.ParseValueString(d, "HeroPositionIcon4");
		o.HeroPositionText4 = BaseJsonMarshaler.ParseValueString(d, "HeroPositionText4");
		o.TranscendRecommend1 = BaseJsonMarshaler.ParseArrayCast(d, "TranscendRecommend1", BaseJsonMarshaler.CastToInt);
		o.TranscendRecommend2 = BaseJsonMarshaler.ParseArrayCast(d, "TranscendRecommend2", BaseJsonMarshaler.CastToInt);
		o.TranscendRecommend3 = BaseJsonMarshaler.ParseArrayCast(d, "TranscendRecommend3", BaseJsonMarshaler.CastToInt);
		o.EquipoptionRecommend1 = BaseJsonMarshaler.ParseArrayCast(d, "EquipoptionRecommend1", BaseJsonMarshaler.CastToString);
		o.EquipoptionRecommend2 = BaseJsonMarshaler.ParseArrayCast(d, "EquipoptionRecommend2", BaseJsonMarshaler.CastToString);
		o.EquipoptionRecommend3 = BaseJsonMarshaler.ParseArrayCast(d, "EquipoptionRecommend3", BaseJsonMarshaler.CastToString);
		o.SoulWeaponBasicSkillIndex = BaseJsonMarshaler.ParseValueInt(d, "SoulWeaponBasicSkillIndex");
		o.Grade = BaseJsonMarshaler.ParseValueByte(d, "Grade");
		o.PetOptionIndices = BaseJsonMarshaler.ParseArrayCast(d, "PetOptionIndices", BaseJsonMarshaler.CastToInt);
		o.PetBonusIndices = BaseJsonMarshaler.ParseArrayCast(d, "PetBonusIndices", BaseJsonMarshaler.CastToInt);
		o.PlayableCharacter = BaseJsonMarshaler.ParseValueBool(d, "PlayableCharacter");
		o.MaxFood = BaseJsonMarshaler.ParseValueInt(d, "MaxFood");
		o.IsLinePet = BaseJsonMarshaler.ParseValueBool(d, "IsLinePet");
		return o;
	}

	public static string EncodeJsonString(CreatureData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CreatureData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		if (o.ReplaceNameKey != null)
		{
			dictionary.Add("ReplaceNameKey", o.ReplaceNameKey);
		}
		dictionary.Add("NameParam", BaseJsonMarshaler.EncodeArray(o.NameParam));
		if (o.CodeName != null)
		{
			dictionary.Add("CodeName", o.CodeName);
		}
		if (o.ModelCode != null)
		{
			dictionary.Add("ModelCode", o.ModelCode);
		}
		if (o.ModelPrefix != null)
		{
			dictionary.Add("ModelPrefix", o.ModelPrefix);
		}
		dictionary.Add("OpenType", o.OpenType.ToString());
		dictionary.Add("OpenStatus", o.OpenStatus.ToString());
		dictionary.Add("OpenDate", o.OpenDate.ToString());
		dictionary.Add("Race", o.Race.ToString());
		dictionary.Add("TagType", o.TagType.ToString());
		dictionary.Add("AttrType", BaseJsonMarshaler.EncodeArray(o.AttrType));
		dictionary.Add("WeakType", BaseJsonMarshaler.EncodeArray(o.WeakType));
		if (o.ArchiveStory != null)
		{
			dictionary.Add("ArchiveStory", o.ArchiveStory);
		}
		dictionary.Add("ArchiveType", o.ArchiveType.ToString());
		dictionary.Add("AttackType", o.AttackType.ToString());
		dictionary.Add("AttackWeakType", BaseJsonMarshaler.EncodeArray(o.AttackWeakType));
		dictionary.Add("PositionType", o.PositionType.ToString());
		dictionary.Add("PositionWeight", o.PositionWeight.ToString());
		dictionary.Add("SizeMm", o.SizeMm.ToString());
		dictionary.Add("BlockDash", o.BlockDash.ToString());
		dictionary.Add("HeightMm", o.HeightMm.ToString());
		dictionary.Add("IgnoreCc", BaseJsonMarshaler.EncodeArray(o.IgnoreCc));
		dictionary.Add("TransformType", o.TransformType.ToString());
		dictionary.Add("AvoidSkillTargetType", BaseJsonMarshaler.EncodeArray(o.AvoidSkillTargetType));
		dictionary.Add("OffsetMx", o.OffsetMx.ToString());
		dictionary.Add("OffsetMz", o.OffsetMz.ToString());
		if (o.GradeStoneItemCode != null)
		{
			dictionary.Add("GradeStoneItemCode", o.GradeStoneItemCode);
		}
		if (o.StarStoneItemCode != null)
		{
			dictionary.Add("StarStoneItemCode", o.StarStoneItemCode);
		}
		dictionary.Add("DropItemWeight", o.DropItemWeight.ToString());
		dictionary.Add("DropGoldWeight", o.DropGoldWeight.ToString());
		dictionary.Add("BaseSkillIndex", o.BaseSkillIndex.ToString());
		dictionary.Add("SkillIndex1", o.SkillIndex1.ToString());
		dictionary.Add("SkillIndex2", o.SkillIndex2.ToString());
		dictionary.Add("SkillIndex3", o.SkillIndex3.ToString());
		dictionary.Add("SkillIndex4", o.SkillIndex4.ToString());
		dictionary.Add("SkillIndex5", o.SkillIndex5.ToString());
		dictionary.Add("LeaderSkillIndex", o.LeaderSkillIndex.ToString());
		dictionary.Add("SkillExtend1", BaseJsonMarshaler.EncodeArray(o.SkillExtend1));
		dictionary.Add("SkillExtend2", BaseJsonMarshaler.EncodeArray(o.SkillExtend2));
		dictionary.Add("SkillExtend3", BaseJsonMarshaler.EncodeArray(o.SkillExtend3));
		dictionary.Add("SkillExtend4", BaseJsonMarshaler.EncodeArray(o.SkillExtend4));
		dictionary.Add("SkillExtend5", BaseJsonMarshaler.EncodeArray(o.SkillExtend5));
		dictionary.Add("LinkedSkillIndex1", BaseJsonMarshaler.EncodeArray(o.LinkedSkillIndex1));
		dictionary.Add("LinkedSkillIndex2", BaseJsonMarshaler.EncodeArray(o.LinkedSkillIndex2));
		dictionary.Add("LinkedSkillIndex3", BaseJsonMarshaler.EncodeArray(o.LinkedSkillIndex3));
		dictionary.Add("LinkedSkillIndex4", BaseJsonMarshaler.EncodeArray(o.LinkedSkillIndex4));
		dictionary.Add("LinkedSkillIndex5", BaseJsonMarshaler.EncodeArray(o.LinkedSkillIndex5));
		dictionary.Add("ExtraSkill", BaseJsonMarshaler.EncodeArray(o.ExtraSkill));
		dictionary.Add("ExtraActiveSkill", BaseJsonMarshaler.EncodeArray(o.ExtraActiveSkill));
		dictionary.Add("ShowInfoSkill", BaseJsonMarshaler.EncodeArray(o.ShowInfoSkill));
		dictionary.Add("GetTranscendSkillPoint", BaseJsonMarshaler.EncodeArray(o.GetTranscendSkillPoint));
		dictionary.Add("TranscendSkill1", BaseJsonMarshaler.EncodeArray(o.TranscendSkill1));
		dictionary.Add("ReqSkillPoint1", BaseJsonMarshaler.EncodeArray(o.ReqSkillPoint1));
		dictionary.Add("TranscendSkill2", BaseJsonMarshaler.EncodeArray(o.TranscendSkill2));
		dictionary.Add("ReqSkillPoint2", BaseJsonMarshaler.EncodeArray(o.ReqSkillPoint2));
		dictionary.Add("TranscendSkill3", BaseJsonMarshaler.EncodeArray(o.TranscendSkill3));
		dictionary.Add("ReqSkillPoint3", BaseJsonMarshaler.EncodeArray(o.ReqSkillPoint3));
		dictionary.Add("TranscendSkill4", BaseJsonMarshaler.EncodeArray(o.TranscendSkill4));
		dictionary.Add("ReqSkillPoint4", BaseJsonMarshaler.EncodeArray(o.ReqSkillPoint4));
		dictionary.Add("TranscendSkill5", BaseJsonMarshaler.EncodeArray(o.TranscendSkill5));
		dictionary.Add("ReqSkillPoint5", BaseJsonMarshaler.EncodeArray(o.ReqSkillPoint5));
		dictionary.Add("StartHeroStar", o.StartHeroStar.ToString());
		dictionary.Add("StartHeroLevel", o.StartHeroLevel.ToString());
		if (o.ReplaceDescKey != null)
		{
			dictionary.Add("ReplaceDescKey", o.ReplaceDescKey);
		}
		dictionary.Add("DescParam", BaseJsonMarshaler.EncodeArray(o.DescParam));
		if (o.ReplaceTitleKey != null)
		{
			dictionary.Add("ReplaceTitleKey", o.ReplaceTitleKey);
		}
		dictionary.Add("TitleParam", BaseJsonMarshaler.EncodeArray(o.TitleParam));
		if (o.ClearTip != null)
		{
			dictionary.Add("ClearTip", o.ClearTip);
		}
		dictionary.Add("AddStamina", o.AddStamina.ToString());
		dictionary.Add("IsDestroyOnDied", o.IsDestroyOnDied.ToString());
		if (o.HeroPositionIcon1 != null)
		{
			dictionary.Add("HeroPositionIcon1", o.HeroPositionIcon1);
		}
		if (o.HeroPositionText1 != null)
		{
			dictionary.Add("HeroPositionText1", o.HeroPositionText1);
		}
		if (o.HeroPositionIcon2 != null)
		{
			dictionary.Add("HeroPositionIcon2", o.HeroPositionIcon2);
		}
		if (o.HeroPositionText2 != null)
		{
			dictionary.Add("HeroPositionText2", o.HeroPositionText2);
		}
		if (o.HeroPositionIcon3 != null)
		{
			dictionary.Add("HeroPositionIcon3", o.HeroPositionIcon3);
		}
		if (o.HeroPositionText3 != null)
		{
			dictionary.Add("HeroPositionText3", o.HeroPositionText3);
		}
		if (o.HeroPositionIcon4 != null)
		{
			dictionary.Add("HeroPositionIcon4", o.HeroPositionIcon4);
		}
		if (o.HeroPositionText4 != null)
		{
			dictionary.Add("HeroPositionText4", o.HeroPositionText4);
		}
		dictionary.Add("TranscendRecommend1", BaseJsonMarshaler.EncodeArray(o.TranscendRecommend1));
		dictionary.Add("TranscendRecommend2", BaseJsonMarshaler.EncodeArray(o.TranscendRecommend2));
		dictionary.Add("TranscendRecommend3", BaseJsonMarshaler.EncodeArray(o.TranscendRecommend3));
		dictionary.Add("EquipoptionRecommend1", BaseJsonMarshaler.EncodeArray(o.EquipoptionRecommend1));
		dictionary.Add("EquipoptionRecommend2", BaseJsonMarshaler.EncodeArray(o.EquipoptionRecommend2));
		dictionary.Add("EquipoptionRecommend3", BaseJsonMarshaler.EncodeArray(o.EquipoptionRecommend3));
		dictionary.Add("SoulWeaponBasicSkillIndex", o.SoulWeaponBasicSkillIndex.ToString());
		dictionary.Add("Grade", o.Grade.ToString());
		dictionary.Add("PetOptionIndices", BaseJsonMarshaler.EncodeArray(o.PetOptionIndices));
		dictionary.Add("PetBonusIndices", BaseJsonMarshaler.EncodeArray(o.PetBonusIndices));
		dictionary.Add("PlayableCharacter", o.PlayableCharacter.ToString());
		dictionary.Add("MaxFood", o.MaxFood.ToString());
		dictionary.Add("IsLinePet", o.IsLinePet.ToString());
		return dictionary;
	}
}
