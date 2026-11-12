using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_JoinPartyNotice
{
	public static JoinPartyNotice Parse(string s)
	{
		return Parse(s, new JoinPartyNotice());
	}

	public static JoinPartyNotice Parse(string s, JoinPartyNotice o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static JoinPartyNotice Parse(IDictionary d)
	{
		return Parse(d, new JoinPartyNotice());
	}

	public static JoinPartyNotice Parse(IDictionary d, JoinPartyNotice o)
	{
		o.PartyInfo = BaseJsonMarshaler.ParseObject(d, "PartyInfo", JM_NShared_NParty_PartyInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(JoinPartyNotice o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(JoinPartyNotice o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PartyInfo", JM_NShared_NParty_PartyInfo.EncodeJsonObject(o.PartyInfo));
		return dictionary;
	}
}
