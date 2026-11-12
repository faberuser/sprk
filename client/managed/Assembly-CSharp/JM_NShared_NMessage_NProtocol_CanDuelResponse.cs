using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_CanDuelResponse
{
	public static CanDuelResponse Parse(string s)
	{
		return Parse(s, new CanDuelResponse());
	}

	public static CanDuelResponse Parse(string s, CanDuelResponse o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CanDuelResponse Parse(IDictionary d)
	{
		return Parse(d, new CanDuelResponse());
	}

	public static CanDuelResponse Parse(IDictionary d, CanDuelResponse o)
	{
		o.CanDuel = BaseJsonMarshaler.ParseValueBool(d, "CanDuel");
		return o;
	}

	public static string EncodeJsonString(CanDuelResponse o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CanDuelResponse o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CanDuel", o.CanDuel.ToString());
		return dictionary;
	}
}
