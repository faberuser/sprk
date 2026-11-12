using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ReceiveMail;

public static class JM_NShared_ReceiveMail_Request
{
	public static NShared.ReceiveMail.Request Parse(string s)
	{
		return Parse(s, new NShared.ReceiveMail.Request());
	}

	public static NShared.ReceiveMail.Request Parse(string s, NShared.ReceiveMail.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ReceiveMail.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ReceiveMail.Request());
	}

	public static NShared.ReceiveMail.Request Parse(IDictionary d, NShared.ReceiveMail.Request o)
	{
		o.MailIndex = BaseJsonMarshaler.ParseValueLong(d, "MailIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ReceiveMail.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ReceiveMail.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MailIndex", o.MailIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
