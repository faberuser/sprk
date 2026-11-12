using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_SetUserNick
{
	public static SetUserNick Parse(string s)
	{
		return Parse(s, new SetUserNick());
	}

	public static SetUserNick Parse(string s, SetUserNick o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static SetUserNick Parse(IDictionary d)
	{
		return Parse(d, new SetUserNick());
	}

	public static SetUserNick Parse(IDictionary d, SetUserNick o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.Nick = BaseJsonMarshaler.ParseValueString(d, "Nick");
		return o;
	}

	public static string EncodeJsonString(SetUserNick o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(SetUserNick o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		if (o.Nick != null)
		{
			dictionary.Add("Nick", o.Nick);
		}
		return dictionary;
	}
}
