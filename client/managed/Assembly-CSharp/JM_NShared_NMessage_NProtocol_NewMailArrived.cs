using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_NewMailArrived
{
	public static NewMailArrived Parse(string s)
	{
		return Parse(s, new NewMailArrived());
	}

	public static NewMailArrived Parse(string s, NewMailArrived o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NewMailArrived Parse(IDictionary d)
	{
		return Parse(d, new NewMailArrived());
	}

	public static NewMailArrived Parse(IDictionary d, NewMailArrived o)
	{
		o.MailIndex = BaseJsonMarshaler.ParseValueLong(d, "MailIndex");
		o.IsGlobal = BaseJsonMarshaler.ParseValueBool(d, "IsGlobal");
		return o;
	}

	public static string EncodeJsonString(NewMailArrived o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NewMailArrived o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MailIndex", o.MailIndex.ToString());
		dictionary.Add("IsGlobal", o.IsGlobal.ToString());
		return dictionary;
	}
}
