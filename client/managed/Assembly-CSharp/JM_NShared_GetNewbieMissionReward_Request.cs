using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetNewbieMissionReward;

public static class JM_NShared_GetNewbieMissionReward_Request
{
	public static NShared.GetNewbieMissionReward.Request Parse(string s)
	{
		return Parse(s, new NShared.GetNewbieMissionReward.Request());
	}

	public static NShared.GetNewbieMissionReward.Request Parse(string s, NShared.GetNewbieMissionReward.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetNewbieMissionReward.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetNewbieMissionReward.Request());
	}

	public static NShared.GetNewbieMissionReward.Request Parse(IDictionary d, NShared.GetNewbieMissionReward.Request o)
	{
		o.MissionIndex = BaseJsonMarshaler.ParseValueInt(d, "MissionIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetNewbieMissionReward.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetNewbieMissionReward.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MissionIndex", o.MissionIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
