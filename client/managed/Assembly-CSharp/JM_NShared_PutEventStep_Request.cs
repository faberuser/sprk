using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.PutEventStep;

public static class JM_NShared_PutEventStep_Request
{
	public static NShared.PutEventStep.Request Parse(string s)
	{
		return Parse(s, new NShared.PutEventStep.Request());
	}

	public static NShared.PutEventStep.Request Parse(string s, NShared.PutEventStep.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.PutEventStep.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.PutEventStep.Request());
	}

	public static NShared.PutEventStep.Request Parse(IDictionary d, NShared.PutEventStep.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.PutEventStep.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.PutEventStep.Request o)
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
