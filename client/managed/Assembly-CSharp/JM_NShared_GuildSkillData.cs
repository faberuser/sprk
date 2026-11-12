using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildSkillData
{
	public static GuildSkillData Parse(string s)
	{
		return Parse(s, new GuildSkillData());
	}

	public static GuildSkillData Parse(string s, GuildSkillData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildSkillData Parse(IDictionary d)
	{
		return Parse(d, new GuildSkillData());
	}

	public static GuildSkillData Parse(IDictionary d, GuildSkillData o)
	{
		o.GuildSkillIndex = BaseJsonMarshaler.ParseValueInt(d, "GuildSkillIndex");
		o.GuildSkillType = BaseJsonMarshaler.ParseEnum<GuildSkillType>(d, "GuildSkillType");
		o.SkillDescParam = BaseJsonMarshaler.ParseValueString(d, "SkillDescParam");
		o.SkillDesc = BaseJsonMarshaler.ParseValueString(d, "SkillDesc");
		o.BonusType = BaseJsonMarshaler.ParseEnum<EventBonusType>(d, "BonusType");
		o.Code = BaseJsonMarshaler.ParseValueString(d, "Code");
		o.ToolTipName = BaseJsonMarshaler.ParseValueString(d, "ToolTipName");
		o.ToolTipDesc = BaseJsonMarshaler.ParseValueString(d, "ToolTipDesc");
		o.BattleTypes = BaseJsonMarshaler.ParseArray<BattleType>(d, "BattleTypes");
		return o;
	}

	public static string EncodeJsonString(GuildSkillData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildSkillData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GuildSkillIndex", o.GuildSkillIndex.ToString());
		dictionary.Add("GuildSkillType", o.GuildSkillType.ToString());
		if (o.SkillDescParam != null)
		{
			dictionary.Add("SkillDescParam", o.SkillDescParam);
		}
		if (o.SkillDesc != null)
		{
			dictionary.Add("SkillDesc", o.SkillDesc);
		}
		dictionary.Add("BonusType", o.BonusType.ToString());
		if (o.Code != null)
		{
			dictionary.Add("Code", o.Code);
		}
		if (o.ToolTipName != null)
		{
			dictionary.Add("ToolTipName", o.ToolTipName);
		}
		if (o.ToolTipDesc != null)
		{
			dictionary.Add("ToolTipDesc", o.ToolTipDesc);
		}
		dictionary.Add("BattleTypes", BaseJsonMarshaler.EncodeArray(o.BattleTypes));
		return dictionary;
	}
}
