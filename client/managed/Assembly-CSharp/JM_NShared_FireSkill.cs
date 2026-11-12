using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_FireSkill
{
	public static string EncodeJsonString(FireSkill o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(FireSkill o)
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
		dictionary.Add("TargetPosition", JM_NShared_NUtil_Vector3.EncodeJsonObject(o.TargetPosition));
		dictionary.Add("TickIndex", o.TickIndex.ToString());
		dictionary.Add("OperationInfos", BaseJsonMarshaler.EncodeArray(o.OperationInfos, JM_NShared_FireSkill_OperationInfo.EncodeJsonObject));
		dictionary.Add("GlobalTimestamp", o.GlobalTimestamp.ToString());
		return dictionary;
	}
}
