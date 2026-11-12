using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_ChatBan
{
	public static ChatBan Parse(string s)
	{
		return Parse(s, new ChatBan());
	}

	public static ChatBan Parse(string s, ChatBan o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ChatBan Parse(IDictionary d)
	{
		return Parse(d, new ChatBan());
	}

	public static ChatBan Parse(IDictionary d, ChatBan o)
	{
		o.BanHour = BaseJsonMarshaler.ParseValueInt(d, "BanHour");
		o.Infos = BaseJsonMarshaler.ParseArray(d, "Infos", JM_NShared_ChatBanInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(ChatBan o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ChatBan o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BanHour", o.BanHour.ToString());
		dictionary.Add("Infos", BaseJsonMarshaler.EncodeArray(o.Infos, JM_NShared_ChatBanInfo.EncodeJsonObject));
		return dictionary;
	}
}
