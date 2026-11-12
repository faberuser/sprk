using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetDispatchList;

public static class JM_NShared_GetDispatchList_Request
{
	public static NShared.GetDispatchList.Request Parse(string s)
	{
		return Parse(s, new NShared.GetDispatchList.Request());
	}

	public static NShared.GetDispatchList.Request Parse(string s, NShared.GetDispatchList.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetDispatchList.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetDispatchList.Request());
	}

	public static NShared.GetDispatchList.Request Parse(IDictionary d, NShared.GetDispatchList.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetDispatchList.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetDispatchList.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
