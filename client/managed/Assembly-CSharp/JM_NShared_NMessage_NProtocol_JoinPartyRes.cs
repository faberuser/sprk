using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_JoinPartyRes
{
	public static JoinPartyRes Parse(string s)
	{
		return Parse(s, new JoinPartyRes());
	}

	public static JoinPartyRes Parse(string s, JoinPartyRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static JoinPartyRes Parse(IDictionary d)
	{
		return Parse(d, new JoinPartyRes());
	}

	public static JoinPartyRes Parse(IDictionary d, JoinPartyRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<JoinPartyRes.ResultType>(d, "Result");
		o.PartyInfo = BaseJsonMarshaler.ParseObject(d, "PartyInfo", JM_NShared_NParty_PartyInfo.Parse);
		o.MemberIndex = BaseJsonMarshaler.ParseValueInt(d, "MemberIndex");
		o.SendKey = BaseJsonMarshaler.ParseValueLong(d, "SendKey");
		return o;
	}

	public static string EncodeJsonString(JoinPartyRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(JoinPartyRes o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Result", o.Result.ToString());
		dictionary.Add("PartyInfo", JM_NShared_NParty_PartyInfo.EncodeJsonObject(o.PartyInfo));
		dictionary.Add("MemberIndex", o.MemberIndex.ToString());
		dictionary.Add("SendKey", o.SendKey.ToString());
		return dictionary;
	}
}
