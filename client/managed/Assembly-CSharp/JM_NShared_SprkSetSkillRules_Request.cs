using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SprkSetSkillRules;

public static class JM_NShared_SprkSetSkillRules_Request
{
	public static NShared.SprkSetSkillRules.Request Parse(string s)
	{
		return Parse(s, new NShared.SprkSetSkillRules.Request());
	}

	public static NShared.SprkSetSkillRules.Request Parse(string s, NShared.SprkSetSkillRules.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SprkSetSkillRules.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SprkSetSkillRules.Request());
	}

	public static NShared.SprkSetSkillRules.Request Parse(IDictionary d, NShared.SprkSetSkillRules.Request o)
	{
		o.SkillRules = BaseJsonMarshaler.ParseValueString(d, "SkillRules");
		o.Replace = BaseJsonMarshaler.ParseValueBool(d, "Replace");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SprkSetSkillRules.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SprkSetSkillRules.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SkillRules", o.SkillRules ?? "{}");
		dictionary.Add("Replace", o.Replace ? "1" : "0");
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
