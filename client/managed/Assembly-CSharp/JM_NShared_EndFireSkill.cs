using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EndFireSkill
{
	public static string EncodeJsonString(EndFireSkill o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EndFireSkill o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("SkillId", o.SkillId.ToString());
		dictionary.Add("IsCancel", o.IsCancel.ToString());
		dictionary.Add("GlobalTimestamp", o.GlobalTimestamp.ToString());
		return dictionary;
	}
}
