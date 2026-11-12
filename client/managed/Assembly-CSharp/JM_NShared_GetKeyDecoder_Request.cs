using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetKeyDecoder;

public static class JM_NShared_GetKeyDecoder_Request
{
	public static NShared.GetKeyDecoder.Request Parse(string s)
	{
		return Parse(s, new NShared.GetKeyDecoder.Request());
	}

	public static NShared.GetKeyDecoder.Request Parse(string s, NShared.GetKeyDecoder.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetKeyDecoder.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetKeyDecoder.Request());
	}

	public static NShared.GetKeyDecoder.Request Parse(IDictionary d, NShared.GetKeyDecoder.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetKeyDecoder.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetKeyDecoder.Request o)
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
