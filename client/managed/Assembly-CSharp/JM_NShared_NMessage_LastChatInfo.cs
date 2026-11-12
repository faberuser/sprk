using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage;

public static class JM_NShared_NMessage_LastChatInfo
{
	public static LastChatInfo Parse(string s)
	{
		return Parse(s, new LastChatInfo());
	}

	public static LastChatInfo Parse(string s, LastChatInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static LastChatInfo Parse(IDictionary d)
	{
		return Parse(d, new LastChatInfo());
	}

	public static LastChatInfo Parse(IDictionary d, LastChatInfo o)
	{
		o.LastChannelChatInfo = BaseJsonMarshaler.ParseObject(d, "LastChannelChatInfo", JM_NShared_NMessage_LastSingleChatInfo.Parse);
		o.LastGuildChatInfo = BaseJsonMarshaler.ParseObject(d, "LastGuildChatInfo", JM_NShared_NMessage_LastSingleChatInfo.Parse);
		o.LastPartyChatInfo = BaseJsonMarshaler.ParseObject(d, "LastPartyChatInfo", JM_NShared_NMessage_LastSingleChatInfo.Parse);
		o.LastWhisperChatInfos = BaseJsonMarshaler.ParseArray(d, "LastWhisperChatInfos", JM_NShared_NMessage_LastSingleChatInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(LastChatInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(LastChatInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("LastChannelChatInfo", JM_NShared_NMessage_LastSingleChatInfo.EncodeJsonObject(o.LastChannelChatInfo));
		dictionary.Add("LastGuildChatInfo", JM_NShared_NMessage_LastSingleChatInfo.EncodeJsonObject(o.LastGuildChatInfo));
		dictionary.Add("LastPartyChatInfo", JM_NShared_NMessage_LastSingleChatInfo.EncodeJsonObject(o.LastPartyChatInfo));
		dictionary.Add("LastWhisperChatInfos", BaseJsonMarshaler.EncodeArray(o.LastWhisperChatInfos, JM_NShared_NMessage_LastSingleChatInfo.EncodeJsonObject));
		return dictionary;
	}
}
