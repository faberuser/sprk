using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_KickedGuildMember
{
	public static KickedGuildMember Parse(string s)
	{
		return Parse(s, new KickedGuildMember());
	}

	public static KickedGuildMember Parse(string s, KickedGuildMember o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static KickedGuildMember Parse(IDictionary d)
	{
		return Parse(d, new KickedGuildMember());
	}

	public static KickedGuildMember Parse(IDictionary d, KickedGuildMember o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.Nick = BaseJsonMarshaler.ParseValueString(d, "Nick");
		o.GuildWithdrawTime = BaseJsonMarshaler.ParseValueString(d, "GuildWithdrawTime");
		return o;
	}

	public static string EncodeJsonString(KickedGuildMember o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(KickedGuildMember o)
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
		if (o.GuildWithdrawTime != null)
		{
			dictionary.Add("GuildWithdrawTime", o.GuildWithdrawTime);
		}
		return dictionary;
	}
}
