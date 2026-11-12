using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ReserveOffSkillReq
{
	public static ReserveOffSkillReq Parse(string s)
	{
		return Parse(s, new ReserveOffSkillReq());
	}

	public static ReserveOffSkillReq Parse(string s, ReserveOffSkillReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ReserveOffSkillReq Parse(IDictionary d)
	{
		return Parse(d, new ReserveOffSkillReq());
	}

	public static ReserveOffSkillReq Parse(IDictionary d, ReserveOffSkillReq o)
	{
		o.CreatureUid = BaseJsonMarshaler.ParseValueInt(d, "CreatureUid");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(ReserveOffSkillReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ReserveOffSkillReq o)
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
