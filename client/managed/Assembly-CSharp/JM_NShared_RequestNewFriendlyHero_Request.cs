using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RequestNewFriendlyHero;

public static class JM_NShared_RequestNewFriendlyHero_Request
{
	public static NShared.RequestNewFriendlyHero.Request Parse(string s)
	{
		return Parse(s, new NShared.RequestNewFriendlyHero.Request());
	}

	public static NShared.RequestNewFriendlyHero.Request Parse(string s, NShared.RequestNewFriendlyHero.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RequestNewFriendlyHero.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RequestNewFriendlyHero.Request());
	}

	public static NShared.RequestNewFriendlyHero.Request Parse(IDictionary d, NShared.RequestNewFriendlyHero.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RequestNewFriendlyHero.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RequestNewFriendlyHero.Request o)
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
