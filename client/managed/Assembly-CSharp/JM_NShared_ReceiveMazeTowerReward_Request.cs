using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ReceiveMazeTowerReward;

public static class JM_NShared_ReceiveMazeTowerReward_Request
{
	public static NShared.ReceiveMazeTowerReward.Request Parse(string s)
	{
		return Parse(s, new NShared.ReceiveMazeTowerReward.Request());
	}

	public static NShared.ReceiveMazeTowerReward.Request Parse(string s, NShared.ReceiveMazeTowerReward.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ReceiveMazeTowerReward.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ReceiveMazeTowerReward.Request());
	}

	public static NShared.ReceiveMazeTowerReward.Request Parse(IDictionary d, NShared.ReceiveMazeTowerReward.Request o)
	{
		o.MazeTowerRewardIndex = BaseJsonMarshaler.ParseValueInt(d, "MazeTowerRewardIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ReceiveMazeTowerReward.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ReceiveMazeTowerReward.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MazeTowerRewardIndex", o.MazeTowerRewardIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
