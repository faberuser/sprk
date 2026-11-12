using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.InitGuildSkill;

public static class JM_NShared_InitGuildSkill_Request
{
	public static NShared.InitGuildSkill.Request Parse(string s)
	{
		return Parse(s, new NShared.InitGuildSkill.Request());
	}

	public static NShared.InitGuildSkill.Request Parse(string s, NShared.InitGuildSkill.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.InitGuildSkill.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.InitGuildSkill.Request());
	}

	public static NShared.InitGuildSkill.Request Parse(IDictionary d, NShared.InitGuildSkill.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.InitGuildSkill.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.InitGuildSkill.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
