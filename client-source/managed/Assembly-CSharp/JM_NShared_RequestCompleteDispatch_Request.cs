using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RequestCompleteDispatch;

public static class JM_NShared_RequestCompleteDispatch_Request
{
	public static NShared.RequestCompleteDispatch.Request Parse(string s)
	{
		return Parse(s, new NShared.RequestCompleteDispatch.Request());
	}

	public static NShared.RequestCompleteDispatch.Request Parse(string s, NShared.RequestCompleteDispatch.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RequestCompleteDispatch.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RequestCompleteDispatch.Request());
	}

	public static NShared.RequestCompleteDispatch.Request Parse(IDictionary d, NShared.RequestCompleteDispatch.Request o)
	{
		o.SlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SlotIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RequestCompleteDispatch.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RequestCompleteDispatch.Request o)
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
