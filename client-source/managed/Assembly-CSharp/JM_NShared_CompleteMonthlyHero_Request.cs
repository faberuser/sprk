using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.CompleteMonthlyHero;

public static class JM_NShared_CompleteMonthlyHero_Request
{
	public static NShared.CompleteMonthlyHero.Request Parse(string s)
	{
		return Parse(s, new NShared.CompleteMonthlyHero.Request());
	}

	public static NShared.CompleteMonthlyHero.Request Parse(string s, NShared.CompleteMonthlyHero.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CompleteMonthlyHero.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.CompleteMonthlyHero.Request());
	}

	public static NShared.CompleteMonthlyHero.Request Parse(IDictionary d, NShared.CompleteMonthlyHero.Request o)
	{
		o.MonthlyHeroIndex = BaseJsonMarshaler.ParseValueInt(d, "MonthlyHeroIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.CompleteMonthlyHero.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.CompleteMonthlyHero.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MonthlyHeroIndex", o.MonthlyHeroIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
