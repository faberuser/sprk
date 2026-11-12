using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_DungeonHeroSetData
{
	public static DungeonHeroSetData Parse(string s)
	{
		return Parse(s, new DungeonHeroSetData());
	}

	public static DungeonHeroSetData Parse(string s, DungeonHeroSetData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static DungeonHeroSetData Parse(IDictionary d)
	{
		return Parse(d, new DungeonHeroSetData());
	}

	public static DungeonHeroSetData Parse(IDictionary d, DungeonHeroSetData o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.Difficulty = BaseJsonMarshaler.ParseEnum<DungeonDifficultyType>(d, "Difficulty");
		o.HeroListSetDataIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroListSetDataIndex");
		o.CommonUniqueWeapon = BaseJsonMarshaler.ParseValueBool(d, "CommonUniqueWeapon");
		o.CommonHeroStar = BaseJsonMarshaler.ParseValueInt(d, "CommonHeroStar");
		o.CommonHeroLevel = BaseJsonMarshaler.ParseValueInt(d, "CommonHeroLevel");
		o.CommonHeroTranscended = BaseJsonMarshaler.ParseValueInt(d, "CommonHeroTranscended");
		o.CommonHeroTier = BaseJsonMarshaler.ParseValueInt(d, "CommonHeroTier");
		o.CommonHeroAddExtraSkill = BaseJsonMarshaler.ParseArrayCast(d, "CommonHeroAddExtraSkill", BaseJsonMarshaler.CastToInt);
		o.IsScenario = BaseJsonMarshaler.ParseValueBool(d, "IsScenario");
		return o;
	}

	public static string EncodeJsonString(DungeonHeroSetData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(DungeonHeroSetData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("Difficulty", o.Difficulty.ToString());
		dictionary.Add("HeroListSetDataIndex", o.HeroListSetDataIndex.ToString());
		dictionary.Add("CommonUniqueWeapon", o.CommonUniqueWeapon.ToString());
		dictionary.Add("CommonHeroStar", o.CommonHeroStar.ToString());
		dictionary.Add("CommonHeroLevel", o.CommonHeroLevel.ToString());
		dictionary.Add("CommonHeroTranscended", o.CommonHeroTranscended.ToString());
		dictionary.Add("CommonHeroTier", o.CommonHeroTier.ToString());
		dictionary.Add("CommonHeroAddExtraSkill", BaseJsonMarshaler.EncodeArray(o.CommonHeroAddExtraSkill));
		dictionary.Add("IsScenario", o.IsScenario.ToString());
		return dictionary;
	}
}
