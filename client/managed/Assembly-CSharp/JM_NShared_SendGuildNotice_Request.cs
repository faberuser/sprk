using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SendGuildNotice;

public static class JM_NShared_SendGuildNotice_Request
{
	public static NShared.SendGuildNotice.Request Parse(string s)
	{
		return Parse(s, new NShared.SendGuildNotice.Request());
	}

	public static NShared.SendGuildNotice.Request Parse(string s, NShared.SendGuildNotice.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SendGuildNotice.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SendGuildNotice.Request());
	}

	public static NShared.SendGuildNotice.Request Parse(IDictionary d, NShared.SendGuildNotice.Request o)
	{
		o.Notice = BaseJsonMarshaler.ParseValueString(d, "Notice");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SendGuildNotice.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SendGuildNotice.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Notice != null)
		{
			dictionary.Add("Notice", o.Notice);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
