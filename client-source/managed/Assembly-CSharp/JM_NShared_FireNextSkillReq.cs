using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_FireNextSkillReq
{
	public static FireNextSkillReq Parse(string s)
	{
		return Parse(s, new FireNextSkillReq());
	}

	public static FireNextSkillReq Parse(string s, FireNextSkillReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static FireNextSkillReq Parse(IDictionary d)
	{
		return Parse(d, new FireNextSkillReq());
	}

	public static FireNextSkillReq Parse(IDictionary d, FireNextSkillReq o)
	{
		o.CreatureUid = BaseJsonMarshaler.ParseValueInt(d, "CreatureUid");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(FireNextSkillReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(FireNextSkillReq o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
