using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlunderReplayInfo
{
	public static PlunderReplayInfo Parse(string s)
	{
		return Parse(s, new PlunderReplayInfo());
	}

	public static PlunderReplayInfo Parse(string s, PlunderReplayInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlunderReplayInfo Parse(IDictionary d)
	{
		return Parse(d, new PlunderReplayInfo());
	}

	public static PlunderReplayInfo Parse(IDictionary d, PlunderReplayInfo o)
	{
		o.FieldName = BaseJsonMarshaler.ParseValueString(d, "FieldName");
		o.Attacker = BaseJsonMarshaler.ParseObject(d, "Attacker", JM_NShared_ReplayTeamInfo.Parse);
		o.Defender = BaseJsonMarshaler.ParseObject(d, "Defender", JM_NShared_ReplayTeamInfo.Parse);
		o.Defended = BaseJsonMarshaler.ParseValueBool(d, "Defended");
		o.StolenItemIndex = BaseJsonMarshaler.ParseValueInt(d, "StolenItemIndex");
		o.StolenItemCount = BaseJsonMarshaler.ParseValueInt(d, "StolenItemCount");
		o.RewardItemIndex = BaseJsonMarshaler.ParseValueInt(d, "RewardItemIndex");
		o.RewardItemCount = BaseJsonMarshaler.ParseValueInt(d, "RewardItemCount");
		o.PlayTimeMs = BaseJsonMarshaler.ParseValueInt(d, "PlayTimeMs");
		o.RecordedTime = BaseJsonMarshaler.ParseValueString(d, "RecordedTime");
		return o;
	}

	public static string EncodeJsonString(PlunderReplayInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlunderReplayInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.FieldName != null)
		{
			dictionary.Add("FieldName", o.FieldName);
		}
		dictionary.Add("Attacker", JM_NShared_ReplayTeamInfo.EncodeJsonObject(o.Attacker));
		dictionary.Add("Defender", JM_NShared_ReplayTeamInfo.EncodeJsonObject(o.Defender));
		dictionary.Add("Defended", o.Defended.ToString());
		dictionary.Add("StolenItemIndex", o.StolenItemIndex.ToString());
		dictionary.Add("StolenItemCount", o.StolenItemCount.ToString());
		dictionary.Add("RewardItemIndex", o.RewardItemIndex.ToString());
		dictionary.Add("RewardItemCount", o.RewardItemCount.ToString());
		dictionary.Add("PlayTimeMs", o.PlayTimeMs.ToString());
		if (o.RecordedTime != null)
		{
			dictionary.Add("RecordedTime", o.RecordedTime);
		}
		return dictionary;
	}
}
