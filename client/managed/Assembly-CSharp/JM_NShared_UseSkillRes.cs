using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_UseSkillRes
{
	public static UseSkillRes Parse(string s)
	{
		return Parse(s, new UseSkillRes());
	}

	public static UseSkillRes Parse(string s, UseSkillRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static UseSkillRes Parse(IDictionary d)
	{
		return Parse(d, new UseSkillRes());
	}

	public static UseSkillRes Parse(IDictionary d, UseSkillRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<UseSkillRes.ResultType>(d, "Result");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(UseSkillRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(UseSkillRes o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Result", o.Result.ToString());
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
