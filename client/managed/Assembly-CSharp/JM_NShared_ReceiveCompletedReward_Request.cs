using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ReceiveCompletedReward;

public static class JM_NShared_ReceiveCompletedReward_Request
{
	public static NShared.ReceiveCompletedReward.Request Parse(string s)
	{
		return Parse(s, new NShared.ReceiveCompletedReward.Request());
	}

	public static NShared.ReceiveCompletedReward.Request Parse(string s, NShared.ReceiveCompletedReward.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ReceiveCompletedReward.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ReceiveCompletedReward.Request());
	}

	public static NShared.ReceiveCompletedReward.Request Parse(IDictionary d, NShared.ReceiveCompletedReward.Request o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndices = BaseJsonMarshaler.ParseArrayCast(d, "DungeonIndices", BaseJsonMarshaler.CastToInt);
		o.EventIndices = BaseJsonMarshaler.ParseArrayCast(d, "EventIndices", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ReceiveCompletedReward.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ReceiveCompletedReward.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndices", BaseJsonMarshaler.EncodeArray(o.DungeonIndices));
		dictionary.Add("EventIndices", BaseJsonMarshaler.EncodeArray(o.EventIndices));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
