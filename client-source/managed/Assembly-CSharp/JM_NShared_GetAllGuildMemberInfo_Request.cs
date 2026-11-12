using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetAllGuildMemberInfo;

public static class JM_NShared_GetAllGuildMemberInfo_Request
{
	public static NShared.GetAllGuildMemberInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetAllGuildMemberInfo.Request());
	}

	public static NShared.GetAllGuildMemberInfo.Request Parse(string s, NShared.GetAllGuildMemberInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetAllGuildMemberInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetAllGuildMemberInfo.Request());
	}

	public static NShared.GetAllGuildMemberInfo.Request Parse(IDictionary d, NShared.GetAllGuildMemberInfo.Request o)
	{
		o.GuildId = BaseJsonMarshaler.ParseValueLong(d, "GuildId");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetAllGuildMemberInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetAllGuildMemberInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GuildId", o.GuildId.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
