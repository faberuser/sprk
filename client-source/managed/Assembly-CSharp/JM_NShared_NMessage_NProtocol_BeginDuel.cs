using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_BeginDuel
{
	public static BeginDuel Parse(string s)
	{
		return Parse(s, new BeginDuel());
	}

	public static BeginDuel Parse(string s, BeginDuel o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BeginDuel Parse(IDictionary d)
	{
		return Parse(d, new BeginDuel());
	}

	public static BeginDuel Parse(IDictionary d, BeginDuel o)
	{
		o.GroupId = BaseJsonMarshaler.ParseValueLong(d, "GroupId");
		return o;
	}

	public static string EncodeJsonString(BeginDuel o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BeginDuel o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GroupId", o.GroupId.ToString());
		return dictionary;
	}
}
