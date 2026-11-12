using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetGuildArenaMemberDeck;

public static class JM_NShared_GetGuildArenaMemberDeck_Request
{
	public static NShared.GetGuildArenaMemberDeck.Request Parse(string s)
	{
		return Parse(s, new NShared.GetGuildArenaMemberDeck.Request());
	}

	public static NShared.GetGuildArenaMemberDeck.Request Parse(string s, NShared.GetGuildArenaMemberDeck.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetGuildArenaMemberDeck.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetGuildArenaMemberDeck.Request());
	}

	public static NShared.GetGuildArenaMemberDeck.Request Parse(IDictionary d, NShared.GetGuildArenaMemberDeck.Request o)
	{
		o.SessionIndex = BaseJsonMarshaler.ParseValueLong(d, "SessionIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetGuildArenaMemberDeck.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetGuildArenaMemberDeck.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SessionIndex", o.SessionIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
