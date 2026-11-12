using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_JoinPartyReqCancel
{
	public static JoinPartyReqCancel Parse(string s)
	{
		return Parse(s, new JoinPartyReqCancel());
	}

	public static JoinPartyReqCancel Parse(string s, JoinPartyReqCancel o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static JoinPartyReqCancel Parse(IDictionary d)
	{
		return Parse(d, new JoinPartyReqCancel());
	}

	public static JoinPartyReqCancel Parse(IDictionary d, JoinPartyReqCancel o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<JoinPartyReqCancel.ResultType>(d, "Result");
		return o;
	}

	public static string EncodeJsonString(JoinPartyReqCancel o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(JoinPartyReqCancel o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Result", o.Result.ToString());
		return dictionary;
	}
}
