using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NContentsDefine;

public static class JM_NShared_NContentsDefine_MatchArena_SeasonDesc
{
	public static MatchArena.SeasonDesc Parse(string s)
	{
		return Parse(s, new MatchArena.SeasonDesc());
	}

	public static MatchArena.SeasonDesc Parse(string s, MatchArena.SeasonDesc o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MatchArena.SeasonDesc Parse(IDictionary d)
	{
		return Parse(d, new MatchArena.SeasonDesc());
	}

	public static MatchArena.SeasonDesc Parse(IDictionary d, MatchArena.SeasonDesc o)
	{
		o.Begin = BaseJsonMarshaler.ParseObject(d, "Begin", JM_NShared_NContentsDefine_MatchArena_TimeWithDayOfWeek.Parse);
		o.End = BaseJsonMarshaler.ParseObject(d, "End", JM_NShared_NContentsDefine_MatchArena_TimeWithDayOfWeek.Parse);
		return o;
	}

	public static string EncodeJsonString(MatchArena.SeasonDesc o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MatchArena.SeasonDesc o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Begin", JM_NShared_NContentsDefine_MatchArena_TimeWithDayOfWeek.EncodeJsonObject(o.Begin));
		dictionary.Add("End", JM_NShared_NContentsDefine_MatchArena_TimeWithDayOfWeek.EncodeJsonObject(o.End));
		return dictionary;
	}
}
