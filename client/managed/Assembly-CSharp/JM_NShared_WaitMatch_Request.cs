using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.WaitMatch;

public static class JM_NShared_WaitMatch_Request
{
	public static NShared.WaitMatch.Request Parse(string s)
	{
		return Parse(s, new NShared.WaitMatch.Request());
	}

	public static NShared.WaitMatch.Request Parse(string s, NShared.WaitMatch.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.WaitMatch.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.WaitMatch.Request());
	}

	public static NShared.WaitMatch.Request Parse(IDictionary d, NShared.WaitMatch.Request o)
	{
		o.PlayOfflineMatch = BaseJsonMarshaler.ParseValueBool(d, "PlayOfflineMatch");
		o.ReserveCancel = BaseJsonMarshaler.ParseValueBool(d, "ReserveCancel");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.WaitMatch.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.WaitMatch.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PlayOfflineMatch", o.PlayOfflineMatch.ToString());
		dictionary.Add("ReserveCancel", o.ReserveCancel.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
