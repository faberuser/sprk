using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.HeroInnResetTime;

public static class JM_NShared_HeroInnResetTime_Request
{
	public static NShared.HeroInnResetTime.Request Parse(string s)
	{
		return Parse(s, new NShared.HeroInnResetTime.Request());
	}

	public static NShared.HeroInnResetTime.Request Parse(string s, NShared.HeroInnResetTime.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.HeroInnResetTime.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.HeroInnResetTime.Request());
	}

	public static NShared.HeroInnResetTime.Request Parse(IDictionary d, NShared.HeroInnResetTime.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.HeroInnResetTime.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.HeroInnResetTime.Request o)
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
