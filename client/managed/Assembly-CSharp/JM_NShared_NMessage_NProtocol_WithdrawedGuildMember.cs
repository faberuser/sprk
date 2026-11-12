using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_WithdrawedGuildMember
{
	public static WithdrawedGuildMember Parse(string s)
	{
		return Parse(s, new WithdrawedGuildMember());
	}

	public static WithdrawedGuildMember Parse(string s, WithdrawedGuildMember o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WithdrawedGuildMember Parse(IDictionary d)
	{
		return Parse(d, new WithdrawedGuildMember());
	}

	public static WithdrawedGuildMember Parse(IDictionary d, WithdrawedGuildMember o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.Nick = BaseJsonMarshaler.ParseValueString(d, "Nick");
		return o;
	}

	public static string EncodeJsonString(WithdrawedGuildMember o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WithdrawedGuildMember o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		if (o.Nick != null)
		{
			dictionary.Add("Nick", o.Nick);
		}
		return dictionary;
	}
}
