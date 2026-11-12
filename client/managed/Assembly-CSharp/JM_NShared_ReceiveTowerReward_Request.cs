using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ReceiveTowerReward;

public static class JM_NShared_ReceiveTowerReward_Request
{
	public static NShared.ReceiveTowerReward.Request Parse(string s)
	{
		return Parse(s, new NShared.ReceiveTowerReward.Request());
	}

	public static NShared.ReceiveTowerReward.Request Parse(string s, NShared.ReceiveTowerReward.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ReceiveTowerReward.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ReceiveTowerReward.Request());
	}

	public static NShared.ReceiveTowerReward.Request Parse(IDictionary d, NShared.ReceiveTowerReward.Request o)
	{
		o.TowerIndex = BaseJsonMarshaler.ParseValueInt(d, "TowerIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ReceiveTowerReward.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ReceiveTowerReward.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("TowerIndex", o.TowerIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
