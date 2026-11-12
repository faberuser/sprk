using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_DuelPong
{
	public static DuelPong Parse(string s)
	{
		return Parse(s, new DuelPong());
	}

	public static DuelPong Parse(string s, DuelPong o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static DuelPong Parse(IDictionary d)
	{
		return Parse(d, new DuelPong());
	}

	public static DuelPong Parse(IDictionary d, DuelPong o)
	{
		o.GroupId = BaseJsonMarshaler.ParseValueLong(d, "GroupId");
		return o;
	}

	public static string EncodeJsonString(DuelPong o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(DuelPong o)
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
