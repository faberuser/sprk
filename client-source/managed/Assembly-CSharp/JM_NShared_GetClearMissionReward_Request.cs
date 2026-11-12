using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetClearMissionReward;

public static class JM_NShared_GetClearMissionReward_Request
{
	public static NShared.GetClearMissionReward.Request Parse(string s)
	{
		return Parse(s, new NShared.GetClearMissionReward.Request());
	}

	public static NShared.GetClearMissionReward.Request Parse(string s, NShared.GetClearMissionReward.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetClearMissionReward.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetClearMissionReward.Request());
	}

	public static NShared.GetClearMissionReward.Request Parse(IDictionary d, NShared.GetClearMissionReward.Request o)
	{
		o.MissionIndex = BaseJsonMarshaler.ParseValueInt(d, "MissionIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetClearMissionReward.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetClearMissionReward.Request o)
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
