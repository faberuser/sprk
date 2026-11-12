using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ReceivePushReward;

public static class JM_NShared_ReceivePushReward_Request
{
	public static NShared.ReceivePushReward.Request Parse(string s)
	{
		return Parse(s, new NShared.ReceivePushReward.Request());
	}

	public static NShared.ReceivePushReward.Request Parse(string s, NShared.ReceivePushReward.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ReceivePushReward.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ReceivePushReward.Request());
	}

	public static NShared.ReceivePushReward.Request Parse(IDictionary d, NShared.ReceivePushReward.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ReceivePushReward.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ReceivePushReward.Request o)
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
