using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BeginFireSkill
{
	public static string EncodeJsonString(BeginFireSkill o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BeginFireSkill o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("SkillId", o.SkillId.ToString());
		dictionary.Add("FiredSkillId", o.FiredSkillId.ToString());
		dictionary.Add("FirstTargetCreatureUid", o.FirstTargetCreatureUid.ToString());
		dictionary.Add("FirePosition", JM_NShared_NUtil_Vector3.EncodeJsonObject(o.FirePosition));
		dictionary.Add("TargetPosition", JM_NShared_NUtil_Vector3.EncodeJsonObject(o.TargetPosition));
		dictionary.Add("DurationTimeMs", o.DurationTimeMs.ToString());
		dictionary.Add("IsIgnoreSoloTime", o.IsIgnoreSoloTime.ToString());
		dictionary.Add("SkillSlotIndex", o.SkillSlotIndex.ToString());
		dictionary.Add("GlobalTimestamp", o.GlobalTimestamp.ToString());
		return dictionary;
	}
}
