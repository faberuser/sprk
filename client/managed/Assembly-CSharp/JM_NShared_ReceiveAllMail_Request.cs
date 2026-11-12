using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ReceiveAllMail;

public static class JM_NShared_ReceiveAllMail_Request
{
	public static NShared.ReceiveAllMail.Request Parse(string s)
	{
		return Parse(s, new NShared.ReceiveAllMail.Request());
	}

	public static NShared.ReceiveAllMail.Request Parse(string s, NShared.ReceiveAllMail.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ReceiveAllMail.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ReceiveAllMail.Request());
	}

	public static NShared.ReceiveAllMail.Request Parse(IDictionary d, NShared.ReceiveAllMail.Request o)
	{
		o.MaxCount = BaseJsonMarshaler.ParseValueInt(d, "MaxCount");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ReceiveAllMail.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ReceiveAllMail.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return SprkMailClaimAll.Encode(dictionary);
		}
		dictionary.Add("MaxCount", o.MaxCount.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return SprkMailClaimAll.Encode(dictionary);
	}
}
