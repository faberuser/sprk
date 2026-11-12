using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BossWarningData
{
	public static BossWarningData Parse(string s)
	{
		return Parse(s, new BossWarningData());
	}

	public static BossWarningData Parse(string s, BossWarningData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BossWarningData Parse(IDictionary d)
	{
		return Parse(d, new BossWarningData());
	}

	public static BossWarningData Parse(IDictionary d, BossWarningData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.SkillIndex = BaseJsonMarshaler.ParseValueInt(d, "SkillIndex");
		o.WarningTargetType = BaseJsonMarshaler.ParseEnum<BossWarningData.EType>(d, "WarningTargetType");
		o.SkillWarningType = BaseJsonMarshaler.ParseEnum<BossWarningData.EWarningType>(d, "SkillWarningType");
		o.SkillWarningOption = BaseJsonMarshaler.ParseValueString(d, "SkillWarningOption");
		o.BuffStartWarningType = BaseJsonMarshaler.ParseValueString(d, "BuffStartWarningType");
		o.BuffStartWarningOption = BaseJsonMarshaler.ParseValueString(d, "BuffStartWarningOption");
		o.BuffStartStackCount = BaseJsonMarshaler.ParseValueInt(d, "BuffStartStackCount");
		o.BuffCrashWarningType = BaseJsonMarshaler.ParseValueString(d, "BuffCrashWarningType");
		o.BuffCrashWarningOption = BaseJsonMarshaler.ParseValueString(d, "BuffCrashWarningOption");
		o.BuffCrashStackCount = BaseJsonMarshaler.ParseValueInt(d, "BuffCrashStackCount");
		o.EffectType = BaseJsonMarshaler.ParseValueString(d, "EffectType");
		o.DelayTime = BaseJsonMarshaler.ParseValueFloat(d, "DelayTime");
		o.DurationTime = BaseJsonMarshaler.ParseValueFloat(d, "DurationTime");
		o.Priority = BaseJsonMarshaler.ParseValueInt(d, "Priority");
		o.CantCancel = BaseJsonMarshaler.ParseValueBool(d, "CantCancel");
		return o;
	}

	public static string EncodeJsonString(BossWarningData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BossWarningData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("SkillIndex", o.SkillIndex.ToString());
		dictionary.Add("WarningTargetType", o.WarningTargetType.ToString());
		dictionary.Add("SkillWarningType", o.SkillWarningType.ToString());
		if (o.SkillWarningOption != null)
		{
			dictionary.Add("SkillWarningOption", o.SkillWarningOption);
		}
		if (o.BuffStartWarningType != null)
		{
			dictionary.Add("BuffStartWarningType", o.BuffStartWarningType);
		}
		if (o.BuffStartWarningOption != null)
		{
			dictionary.Add("BuffStartWarningOption", o.BuffStartWarningOption);
		}
		dictionary.Add("BuffStartStackCount", o.BuffStartStackCount.ToString());
		if (o.BuffCrashWarningType != null)
		{
			dictionary.Add("BuffCrashWarningType", o.BuffCrashWarningType);
		}
		if (o.BuffCrashWarningOption != null)
		{
			dictionary.Add("BuffCrashWarningOption", o.BuffCrashWarningOption);
		}
		dictionary.Add("BuffCrashStackCount", o.BuffCrashStackCount.ToString());
		if (o.EffectType != null)
		{
			dictionary.Add("EffectType", o.EffectType);
		}
		dictionary.Add("DelayTime", o.DelayTime.ToString());
		dictionary.Add("DurationTime", o.DurationTime.ToString());
		dictionary.Add("Priority", o.Priority.ToString());
		dictionary.Add("CantCancel", o.CantCancel.ToString());
		return dictionary;
	}
}
