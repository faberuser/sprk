using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.CheckNewMail;

public static class JM_NShared_CheckNewMail_Request
{
	public static NShared.CheckNewMail.Request Parse(string s)
	{
		return Parse(s, new NShared.CheckNewMail.Request());
	}

	public static NShared.CheckNewMail.Request Parse(string s, NShared.CheckNewMail.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CheckNewMail.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.CheckNewMail.Request());
	}

	public static NShared.CheckNewMail.Request Parse(IDictionary d, NShared.CheckNewMail.Request o)
	{
		o.TopmostMailIndex = BaseJsonMarshaler.ParseValueLong(d, "TopmostMailIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.CheckNewMail.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.CheckNewMail.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("TopmostMailIndex", o.TopmostMailIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
