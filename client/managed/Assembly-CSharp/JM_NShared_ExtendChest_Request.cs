using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ExtendChest;

public static class JM_NShared_ExtendChest_Request
{
	public static NShared.ExtendChest.Request Parse(string s)
	{
		return Parse(s, new NShared.ExtendChest.Request());
	}

	public static NShared.ExtendChest.Request Parse(string s, NShared.ExtendChest.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ExtendChest.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ExtendChest.Request());
	}

	public static NShared.ExtendChest.Request Parse(IDictionary d, NShared.ExtendChest.Request o)
	{
		o.ReqGem = BaseJsonMarshaler.ParseValueInt(d, "ReqGem");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ExtendChest.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ExtendChest.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ReqGem", o.ReqGem.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
