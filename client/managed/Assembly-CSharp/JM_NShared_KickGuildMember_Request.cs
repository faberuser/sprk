using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.KickGuildMember;

public static class JM_NShared_KickGuildMember_Request
{
	public static NShared.KickGuildMember.Request Parse(string s)
	{
		return Parse(s, new NShared.KickGuildMember.Request());
	}

	public static NShared.KickGuildMember.Request Parse(string s, NShared.KickGuildMember.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.KickGuildMember.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.KickGuildMember.Request());
	}

	public static NShared.KickGuildMember.Request Parse(IDictionary d, NShared.KickGuildMember.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.KickGuildMember.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.KickGuildMember.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
