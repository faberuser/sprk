using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_AcceptDuel
{
	public static AcceptDuel Parse(string s)
	{
		return Parse(s, new AcceptDuel());
	}

	public static AcceptDuel Parse(string s, AcceptDuel o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static AcceptDuel Parse(IDictionary d)
	{
		return Parse(d, new AcceptDuel());
	}

	public static AcceptDuel Parse(IDictionary d, AcceptDuel o)
	{
		o.GroupId = BaseJsonMarshaler.ParseValueLong(d, "GroupId");
		return o;
	}

	public static string EncodeJsonString(AcceptDuel o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(AcceptDuel o)
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
