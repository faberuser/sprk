using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ReplayTeamHeroInfo
{
	public static ReplayTeamHeroInfo Parse(string s)
	{
		return Parse(s, new ReplayTeamHeroInfo());
	}

	public static ReplayTeamHeroInfo Parse(string s, ReplayTeamHeroInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ReplayTeamHeroInfo Parse(IDictionary d)
	{
		return Parse(d, new ReplayTeamHeroInfo());
	}

	public static ReplayTeamHeroInfo Parse(IDictionary d, ReplayTeamHeroInfo o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Star = BaseJsonMarshaler.ParseValueInt(d, "Star");
		return o;
	}

	public static string EncodeJsonString(ReplayTeamHeroInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ReplayTeamHeroInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("Star", o.Star.ToString());
		return dictionary;
	}
}
