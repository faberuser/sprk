using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_OnSkillFire
{
	public static OnSkillFire Parse(string s)
	{
		return Parse(s, new OnSkillFire());
	}

	public static OnSkillFire Parse(string s, OnSkillFire o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OnSkillFire Parse(IDictionary d)
	{
		return Parse(d, new OnSkillFire());
	}

	public static OnSkillFire Parse(IDictionary d, OnSkillFire o)
	{
		o.SkillId = BaseJsonMarshaler.ParseValueInt(d, "SkillId");
		return o;
	}

	public static string EncodeJsonString(OnSkillFire o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OnSkillFire o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SkillId", o.SkillId.ToString());
		return dictionary;
	}
}
