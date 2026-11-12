using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_JoinedGuild
{
	public static JoinedGuild Parse(string s)
	{
		return Parse(s, new JoinedGuild());
	}

	public static JoinedGuild Parse(string s, JoinedGuild o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static JoinedGuild Parse(IDictionary d)
	{
		return Parse(d, new JoinedGuild());
	}

	public static JoinedGuild Parse(IDictionary d, JoinedGuild o)
	{
		o.MemberInfo = BaseJsonMarshaler.ParseObject(d, "MemberInfo", JM_NShared_GuildMemberInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(JoinedGuild o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(JoinedGuild o)
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
