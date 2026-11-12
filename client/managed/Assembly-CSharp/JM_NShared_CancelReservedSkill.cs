using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CancelReservedSkill
{
	public static string EncodeJsonString(CancelReservedSkill o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CancelReservedSkill o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("SkillId", o.SkillId.ToString());
		dictionary.Add("GlobalTimestamp", o.GlobalTimestamp.ToString());
		return dictionary;
	}
}
