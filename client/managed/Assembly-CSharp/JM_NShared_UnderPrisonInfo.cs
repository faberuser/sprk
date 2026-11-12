using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_UnderPrisonInfo
{
	public static UnderPrisonInfo Parse(string s)
	{
		return Parse(s, new UnderPrisonInfo());
	}

	public static UnderPrisonInfo Parse(string s, UnderPrisonInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static UnderPrisonInfo Parse(IDictionary d)
	{
		return Parse(d, new UnderPrisonInfo());
	}

	public static UnderPrisonInfo Parse(IDictionary d, UnderPrisonInfo o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.LastCompletedTime = BaseJsonMarshaler.ParseValueString(d, "LastCompletedTime");
		o.CompletedCount = BaseJsonMarshaler.ParseValueInt(d, "CompletedCount");
		o.Diff = BaseJsonMarshaler.ParseValueInt(d, "Diff");
		o.MaxTryCount = BaseJsonMarshaler.ParseValueInt(d, "MaxTryCount");
		return o;
	}

	public static string EncodeJsonString(UnderPrisonInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(UnderPrisonInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		if (o.LastCompletedTime != null)
		{
			dictionary.Add("LastCompletedTime", o.LastCompletedTime);
		}
		dictionary.Add("CompletedCount", o.CompletedCount.ToString());
		dictionary.Add("Diff", o.Diff.ToString());
		dictionary.Add("MaxTryCount", o.MaxTryCount.ToString());
		return dictionary;
	}
}
