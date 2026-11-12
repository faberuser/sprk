using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.CompleteCalculateDispatchResult;

public static class JM_NShared_CompleteCalculateDispatchResult_Request
{
	public static NShared.CompleteCalculateDispatchResult.Request Parse(string s)
	{
		return Parse(s, new NShared.CompleteCalculateDispatchResult.Request());
	}

	public static NShared.CompleteCalculateDispatchResult.Request Parse(string s, NShared.CompleteCalculateDispatchResult.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CompleteCalculateDispatchResult.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.CompleteCalculateDispatchResult.Request());
	}

	public static NShared.CompleteCalculateDispatchResult.Request Parse(IDictionary d, NShared.CompleteCalculateDispatchResult.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.CompleteCalculateDispatchResult.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.CompleteCalculateDispatchResult.Request o)
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
