using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetCertificate;

public static class JM_NShared_GetCertificate_Request
{
	public static NShared.GetCertificate.Request Parse(string s)
	{
		return Parse(s, new NShared.GetCertificate.Request());
	}

	public static NShared.GetCertificate.Request Parse(string s, NShared.GetCertificate.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetCertificate.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetCertificate.Request());
	}

	public static NShared.GetCertificate.Request Parse(IDictionary d, NShared.GetCertificate.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetCertificate.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetCertificate.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
