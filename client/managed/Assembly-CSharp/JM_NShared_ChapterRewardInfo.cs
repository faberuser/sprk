using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ChapterRewardInfo
{
	public static ChapterRewardInfo Parse(string s)
	{
		return Parse(s, new ChapterRewardInfo());
	}

	public static ChapterRewardInfo Parse(string s, ChapterRewardInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ChapterRewardInfo Parse(IDictionary d)
	{
		return Parse(d, new ChapterRewardInfo());
	}

	public static ChapterRewardInfo Parse(IDictionary d, ChapterRewardInfo o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.LastRewardDiff = BaseJsonMarshaler.ParseValueByte(d, "LastRewardDiff");
		return o;
	}

	public static string EncodeJsonString(ChapterRewardInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ChapterRewardInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("LastRewardDiff", o.LastRewardDiff.ToString());
		return dictionary;
	}
}
