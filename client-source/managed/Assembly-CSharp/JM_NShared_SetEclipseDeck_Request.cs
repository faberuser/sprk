using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SetEclipseDeck;

public static class JM_NShared_SetEclipseDeck_Request
{
	public static NShared.SetEclipseDeck.Request Parse(string s)
	{
		return Parse(s, new NShared.SetEclipseDeck.Request());
	}

	public static NShared.SetEclipseDeck.Request Parse(string s, NShared.SetEclipseDeck.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SetEclipseDeck.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SetEclipseDeck.Request());
	}

	public static NShared.SetEclipseDeck.Request Parse(IDictionary d, NShared.SetEclipseDeck.Request o)
	{
		o.HeroInfos = BaseJsonMarshaler.ParseValueString(d, "HeroInfos");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SetEclipseDeck.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SetEclipseDeck.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.HeroInfos != null)
		{
			dictionary.Add("HeroInfos", o.HeroInfos);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
