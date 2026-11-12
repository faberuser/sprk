using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage;

public static class JM_NShared_NMessage_LastSingleChatInfo
{
	public static LastSingleChatInfo Parse(string s)
	{
		return Parse(s, new LastSingleChatInfo());
	}

	public static LastSingleChatInfo Parse(string s, LastSingleChatInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static LastSingleChatInfo Parse(IDictionary d)
	{
		return Parse(d, new LastSingleChatInfo());
	}

	public static LastSingleChatInfo Parse(IDictionary d, LastSingleChatInfo o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.ReadTime = BaseJsonMarshaler.ParseValueString(d, "ReadTime");
		return o;
	}

	public static string EncodeJsonString(LastSingleChatInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(LastSingleChatInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		if (o.ReadTime != null)
		{
			dictionary.Add("ReadTime", o.ReadTime);
		}
		return dictionary;
	}
}
