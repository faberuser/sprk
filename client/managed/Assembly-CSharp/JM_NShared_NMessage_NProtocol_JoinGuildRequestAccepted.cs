using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_JoinGuildRequestAccepted
{
	public static JoinGuildRequestAccepted Parse(string s)
	{
		return Parse(s, new JoinGuildRequestAccepted());
	}

	public static JoinGuildRequestAccepted Parse(string s, JoinGuildRequestAccepted o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static JoinGuildRequestAccepted Parse(IDictionary d)
	{
		return Parse(d, new JoinGuildRequestAccepted());
	}

	public static JoinGuildRequestAccepted Parse(IDictionary d, JoinGuildRequestAccepted o)
	{
		o.GuildId = BaseJsonMarshaler.ParseValueLong(d, "GuildId");
		return o;
	}

	public static string EncodeJsonString(JoinGuildRequestAccepted o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(JoinGuildRequestAccepted o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GuildId", o.GuildId.ToString());
		return dictionary;
	}
}
