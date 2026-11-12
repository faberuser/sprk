using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ChangeNick;

public static class JM_NShared_ChangeNick_Request
{
	public static NShared.ChangeNick.Request Parse(string s)
	{
		return Parse(s, new NShared.ChangeNick.Request());
	}

	public static NShared.ChangeNick.Request Parse(string s, NShared.ChangeNick.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ChangeNick.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ChangeNick.Request());
	}

	public static NShared.ChangeNick.Request Parse(IDictionary d, NShared.ChangeNick.Request o)
	{
		o.Nick = BaseJsonMarshaler.ParseValueString(d, "Nick");
		o.CountryCode = BaseJsonMarshaler.ParseValueString(d, "CountryCode");
		o.Language = BaseJsonMarshaler.ParseValueString(d, "Language");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ChangeNick.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ChangeNick.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Nick != null)
		{
			dictionary.Add("Nick", o.Nick);
		}
		if (o.CountryCode != null)
		{
			dictionary.Add("CountryCode", o.CountryCode);
		}
		if (o.Language != null)
		{
			dictionary.Add("Language", o.Language);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
