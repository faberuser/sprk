using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RewardClearChapter;

public static class JM_NShared_RewardClearChapter_Request
{
	public static NShared.RewardClearChapter.Request Parse(string s)
	{
		return Parse(s, new NShared.RewardClearChapter.Request());
	}

	public static NShared.RewardClearChapter.Request Parse(string s, NShared.RewardClearChapter.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RewardClearChapter.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RewardClearChapter.Request());
	}

	public static NShared.RewardClearChapter.Request Parse(IDictionary d, NShared.RewardClearChapter.Request o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.Step = BaseJsonMarshaler.ParseValueInt(d, "Step");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RewardClearChapter.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RewardClearChapter.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("Step", o.Step.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
