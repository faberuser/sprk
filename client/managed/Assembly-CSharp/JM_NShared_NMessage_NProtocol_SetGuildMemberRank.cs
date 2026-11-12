using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_SetGuildMemberRank
{
	public static SetGuildMemberRank Parse(string s)
	{
		return Parse(s, new SetGuildMemberRank());
	}

	public static SetGuildMemberRank Parse(string s, SetGuildMemberRank o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static SetGuildMemberRank Parse(IDictionary d)
	{
		return Parse(d, new SetGuildMemberRank());
	}

	public static SetGuildMemberRank Parse(IDictionary d, SetGuildMemberRank o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.Rank = BaseJsonMarshaler.ParseValueByte(d, "Rank");
		return o;
	}

	public static string EncodeJsonString(SetGuildMemberRank o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(SetGuildMemberRank o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("Rank", o.Rank.ToString());
		return dictionary;
	}
}
