using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetPushReward;

public static class JM_NShared_GetPushReward_Request
{
	public static NShared.GetPushReward.Request Parse(string s)
	{
		return Parse(s, new NShared.GetPushReward.Request());
	}

	public static NShared.GetPushReward.Request Parse(string s, NShared.GetPushReward.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetPushReward.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetPushReward.Request());
	}

	public static NShared.GetPushReward.Request Parse(IDictionary d, NShared.GetPushReward.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetPushReward.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetPushReward.Request o)
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
