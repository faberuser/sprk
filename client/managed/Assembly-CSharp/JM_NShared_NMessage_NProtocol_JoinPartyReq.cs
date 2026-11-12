using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_JoinPartyReq
{
	public static JoinPartyReq Parse(string s)
	{
		return Parse(s, new JoinPartyReq());
	}

	public static JoinPartyReq Parse(string s, JoinPartyReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static JoinPartyReq Parse(IDictionary d)
	{
		return Parse(d, new JoinPartyReq());
	}

	public static JoinPartyReq Parse(IDictionary d, JoinPartyReq o)
	{
		o.Invitee = BaseJsonMarshaler.ParseObject(d, "Invitee", JM_NShared_NParty_MemberInfo.Parse);
		o.Accepted = BaseJsonMarshaler.ParseValueBool(d, "Accepted");
		o.RoomNo = BaseJsonMarshaler.ParseValueLong(d, "RoomNo");
		o.Result = BaseJsonMarshaler.ParseEnum<JoinPartyReq.ResultType>(d, "Result");
		o.SendKey = BaseJsonMarshaler.ParseValueLong(d, "SendKey");
		return o;
	}

	public static string EncodeJsonString(JoinPartyReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(JoinPartyReq o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Invitee", JM_NShared_NParty_MemberInfo.EncodeJsonObject(o.Invitee));
		dictionary.Add("Accepted", o.Accepted.ToString());
		dictionary.Add("RoomNo", o.RoomNo.ToString());
		dictionary.Add("Result", o.Result.ToString());
		dictionary.Add("SendKey", o.SendKey.ToString());
		return dictionary;
	}
}
