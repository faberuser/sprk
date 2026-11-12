using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.UpgradeHeroSkill;

public static class JM_NShared_UpgradeHeroSkill_Request
{
	public static NShared.UpgradeHeroSkill.Request Parse(string s)
	{
		return Parse(s, new NShared.UpgradeHeroSkill.Request());
	}

	public static NShared.UpgradeHeroSkill.Request Parse(string s, NShared.UpgradeHeroSkill.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.UpgradeHeroSkill.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.UpgradeHeroSkill.Request());
	}

	public static NShared.UpgradeHeroSkill.Request Parse(IDictionary d, NShared.UpgradeHeroSkill.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.SkillIndices = BaseJsonMarshaler.ParseArrayCast(d, "SkillIndices", BaseJsonMarshaler.CastToInt);
		o.TargetLevels = BaseJsonMarshaler.ParseArrayCast(d, "TargetLevels", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.UpgradeHeroSkill.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.UpgradeHeroSkill.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("SkillIndices", BaseJsonMarshaler.EncodeArray(o.SkillIndices));
		dictionary.Add("TargetLevels", BaseJsonMarshaler.EncodeArray(o.TargetLevels));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
