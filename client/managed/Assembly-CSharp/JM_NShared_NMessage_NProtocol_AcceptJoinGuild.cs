using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_AcceptJoinGuild
{
	public static AcceptJoinGuild Parse(string s)
	{
		return Parse(s, new AcceptJoinGuild());
	}

	public static AcceptJoinGuild Parse(string s, AcceptJoinGuild o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static AcceptJoinGuild Parse(IDictionary d)
	{
		return Parse(d, new AcceptJoinGuild());
	}

	public static AcceptJoinGuild Parse(IDictionary d, AcceptJoinGuild o)
	{
		o.MemberInfo = BaseJsonMarshaler.ParseObject(d, "MemberInfo", JM_NShared_GuildMemberInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(AcceptJoinGuild o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(AcceptJoinGuild o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MemberInfo", JM_NShared_GuildMemberInfo.EncodeJsonObject(o.MemberInfo));
		return dictionary;
	}
}
