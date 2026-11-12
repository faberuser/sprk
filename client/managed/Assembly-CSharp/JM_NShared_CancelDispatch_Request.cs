using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.CancelDispatch;

public static class JM_NShared_CancelDispatch_Request
{
	public static NShared.CancelDispatch.Request Parse(string s)
	{
		return Parse(s, new NShared.CancelDispatch.Request());
	}

	public static NShared.CancelDispatch.Request Parse(string s, NShared.CancelDispatch.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CancelDispatch.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.CancelDispatch.Request());
	}

	public static NShared.CancelDispatch.Request Parse(IDictionary d, NShared.CancelDispatch.Request o)
	{
		o.SlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SlotIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.CancelDispatch.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.CancelDispatch.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SlotIndex", o.SlotIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
