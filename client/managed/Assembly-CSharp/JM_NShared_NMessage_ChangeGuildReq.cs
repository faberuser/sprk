using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage;

public static class JM_NShared_NMessage_ChangeGuildReq
{
	public static ChangeGuildReq Parse(string s)
	{
		return Parse(s, new ChangeGuildReq());
	}

	public static ChangeGuildReq Parse(string s, ChangeGuildReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ChangeGuildReq Parse(IDictionary d)
	{
		return Parse(d, new ChangeGuildReq());
	}

	public static ChangeGuildReq Parse(IDictionary d, ChangeGuildReq o)
	{
		o.GuildId = BaseJsonMarshaler.ParseValueLong(d, "GuildId");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(ChangeGuildReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ChangeGuildReq o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GuildId", o.GuildId.ToString());
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
