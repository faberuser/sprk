using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_DuelPing
{
	public static DuelPing Parse(string s)
	{
		return Parse(s, new DuelPing());
	}

	public static DuelPing Parse(string s, DuelPing o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static DuelPing Parse(IDictionary d)
	{
		return Parse(d, new DuelPing());
	}

	public static DuelPing Parse(IDictionary d, DuelPing o)
	{
		o.GroupId = BaseJsonMarshaler.ParseValueLong(d, "GroupId");
		return o;
	}

	public static string EncodeJsonString(DuelPing o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(DuelPing o)
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
