using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_SubQuestInfo
{
	public static SubQuestInfo Parse(string s)
	{
		return Parse(s, new SubQuestInfo());
	}

	public static SubQuestInfo Parse(string s, SubQuestInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static SubQuestInfo Parse(IDictionary d)
	{
		return Parse(d, new SubQuestInfo());
	}

	public static SubQuestInfo Parse(IDictionary d, SubQuestInfo o)
	{
		o.SubQuestIndex = BaseJsonMarshaler.ParseValueInt(d, "SubQuestIndex");
		o.LastStep = BaseJsonMarshaler.ParseValueInt(d, "LastStep");
		o.Progress = BaseJsonMarshaler.ParseValueLong(d, "Progress");
		return o;
	}

	public static string EncodeJsonString(SubQuestInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(SubQuestInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SubQuestIndex", o.SubQuestIndex.ToString());
		dictionary.Add("LastStep", o.LastStep.ToString());
		dictionary.Add("Progress", o.Progress.ToString());
		return dictionary;
	}
}
