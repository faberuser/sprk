using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetEclipseDeck;

public static class JM_NShared_GetEclipseDeck_Request
{
	public static NShared.GetEclipseDeck.Request Parse(string s)
	{
		return Parse(s, new NShared.GetEclipseDeck.Request());
	}

	public static NShared.GetEclipseDeck.Request Parse(string s, NShared.GetEclipseDeck.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetEclipseDeck.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetEclipseDeck.Request());
	}

	public static NShared.GetEclipseDeck.Request Parse(IDictionary d, NShared.GetEclipseDeck.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetEclipseDeck.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetEclipseDeck.Request o)
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
