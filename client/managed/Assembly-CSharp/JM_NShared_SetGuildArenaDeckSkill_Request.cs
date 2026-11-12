using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SetGuildArenaDeckSkill;

public static class JM_NShared_SetGuildArenaDeckSkill_Request
{
	public static NShared.SetGuildArenaDeckSkill.Request Parse(string s)
	{
		return Parse(s, new NShared.SetGuildArenaDeckSkill.Request());
	}

	public static NShared.SetGuildArenaDeckSkill.Request Parse(string s, NShared.SetGuildArenaDeckSkill.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SetGuildArenaDeckSkill.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SetGuildArenaDeckSkill.Request());
	}

	public static NShared.SetGuildArenaDeckSkill.Request Parse(IDictionary d, NShared.SetGuildArenaDeckSkill.Request o)
	{
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.SkillSlotIndices1 = BaseJsonMarshaler.ParseArrayCast(d, "SkillSlotIndices1", BaseJsonMarshaler.CastToInt);
		o.SkillSlotIndices2 = BaseJsonMarshaler.ParseArrayCast(d, "SkillSlotIndices2", BaseJsonMarshaler.CastToInt);
		o.SkillSlotIndices3 = BaseJsonMarshaler.ParseArrayCast(d, "SkillSlotIndices3", BaseJsonMarshaler.CastToInt);
		o.SkillSlotIndices4 = BaseJsonMarshaler.ParseArrayCast(d, "SkillSlotIndices4", BaseJsonMarshaler.CastToInt);
		o.SkillSlotIndices5 = BaseJsonMarshaler.ParseArrayCast(d, "SkillSlotIndices5", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SetGuildArenaDeckSkill.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SetGuildArenaDeckSkill.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		dictionary.Add("SkillSlotIndices1", BaseJsonMarshaler.EncodeArray(o.SkillSlotIndices1));
		dictionary.Add("SkillSlotIndices2", BaseJsonMarshaler.EncodeArray(o.SkillSlotIndices2));
		dictionary.Add("SkillSlotIndices3", BaseJsonMarshaler.EncodeArray(o.SkillSlotIndices3));
		dictionary.Add("SkillSlotIndices4", BaseJsonMarshaler.EncodeArray(o.SkillSlotIndices4));
		dictionary.Add("SkillSlotIndices5", BaseJsonMarshaler.EncodeArray(o.SkillSlotIndices5));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
