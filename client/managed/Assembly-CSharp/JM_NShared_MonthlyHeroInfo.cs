using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_MonthlyHeroInfo
{
	public static MonthlyHeroInfo Parse(string s)
	{
		return Parse(s, new MonthlyHeroInfo());
	}

	public static MonthlyHeroInfo Parse(string s, MonthlyHeroInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MonthlyHeroInfo Parse(IDictionary d)
	{
		return Parse(d, new MonthlyHeroInfo());
	}

	public static MonthlyHeroInfo Parse(IDictionary d, MonthlyHeroInfo o)
	{
		o.MonthlyHeroIndex = BaseJsonMarshaler.ParseValueInt(d, "MonthlyHeroIndex");
		o.CompletedTime = BaseJsonMarshaler.ParseValueString(d, "CompletedTime");
		return o;
	}

	public static string EncodeJsonString(MonthlyHeroInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MonthlyHeroInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MonthlyHeroIndex", o.MonthlyHeroIndex.ToString());
		if (o.CompletedTime != null)
		{
			dictionary.Add("CompletedTime", o.CompletedTime);
		}
		return dictionary;
	}
}
