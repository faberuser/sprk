using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CastSkill
{
	public static string EncodeJsonString(CastSkill o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CastSkill o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("SkillId", o.SkillId.ToString());
		dictionary.Add("SkillLevel", o.SkillLevel.ToString());
		dictionary.Add("TargetCreatureUids", BaseJsonMarshaler.EncodeArray(o.TargetCreatureUids));
		dictionary.Add("GlobalTimestamp", o.GlobalTimestamp.ToString());
		return dictionary;
	}
}
