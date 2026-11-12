using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ChatBanInfo
{
	public static ChatBanInfo Parse(string s)
	{
		return Parse(s, new ChatBanInfo());
	}

	public static ChatBanInfo Parse(string s, ChatBanInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ChatBanInfo Parse(IDictionary d)
	{
		return Parse(d, new ChatBanInfo());
	}

	public static ChatBanInfo Parse(IDictionary d, ChatBanInfo o)
	{
		o.BanType = BaseJsonMarshaler.ParseEnum<ChatBanType>(d, "BanType");
		o.BanReason = BaseJsonMarshaler.ParseEnum<ChatBanReason>(d, "BanReason");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		return o;
	}

	public static string EncodeJsonString(ChatBanInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ChatBanInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BanType", o.BanType.ToString());
		dictionary.Add("BanReason", o.BanReason.ToString());
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		return dictionary;
	}
}
