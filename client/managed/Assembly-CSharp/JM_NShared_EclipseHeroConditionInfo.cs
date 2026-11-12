using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EclipseHeroConditionInfo
{
	public static EclipseHeroConditionInfo Parse(string s)
	{
		return Parse(s, new EclipseHeroConditionInfo());
	}

	public static EclipseHeroConditionInfo Parse(string s, EclipseHeroConditionInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EclipseHeroConditionInfo Parse(IDictionary d)
	{
		return Parse(d, new EclipseHeroConditionInfo());
	}

	public static EclipseHeroConditionInfo Parse(IDictionary d, EclipseHeroConditionInfo o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.Hp = BaseJsonMarshaler.ParseValueLong(d, "Hp");
		o.Mp = BaseJsonMarshaler.ParseValueInt(d, "Mp");
		return o;
	}

	public static string EncodeJsonString(EclipseHeroConditionInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EclipseHeroConditionInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("Hp", o.Hp.ToString());
		dictionary.Add("Mp", o.Mp.ToString());
		return dictionary;
	}
}
