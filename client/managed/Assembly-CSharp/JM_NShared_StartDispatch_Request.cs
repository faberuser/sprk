using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.StartDispatch;

public static class JM_NShared_StartDispatch_Request
{
	public static NShared.StartDispatch.Request Parse(string s)
	{
		return Parse(s, new NShared.StartDispatch.Request());
	}

	public static NShared.StartDispatch.Request Parse(string s, NShared.StartDispatch.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.StartDispatch.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.StartDispatch.Request());
	}

	public static NShared.StartDispatch.Request Parse(IDictionary d, NShared.StartDispatch.Request o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.DeckIndex = BaseJsonMarshaler.ParseValueInt(d, "DeckIndex");
		o.Difficulty = BaseJsonMarshaler.ParseValueInt(d, "Difficulty");
		o.HeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndices", BaseJsonMarshaler.CastToInt);
		o.RepeatCount = BaseJsonMarshaler.ParseValueInt(d, "RepeatCount");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.StartDispatch.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.StartDispatch.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("DeckIndex", o.DeckIndex.ToString());
		dictionary.Add("Difficulty", o.Difficulty.ToString());
		dictionary.Add("HeroIndices", BaseJsonMarshaler.EncodeArray(o.HeroIndices));
		dictionary.Add("RepeatCount", o.RepeatCount.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
