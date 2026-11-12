using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ChangeState
{
	public static string EncodeJsonString(ChangeState o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ChangeState o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("OperationUid", o.OperationUid.ToString());
		dictionary.Add("SkillIndex", o.SkillIndex.ToString());
		dictionary.Add("StateAdded", o.StateAdded.ToString());
		if (o.ChangedStateType != null)
		{
			dictionary.Add("ChangedStateType", o.ChangedStateType);
		}
		dictionary.Add("FinalStateType", BaseJsonMarshaler.EncodeArray(o.FinalStateType));
		dictionary.Add("StackCount", o.StackCount.ToString());
		dictionary.Add("FiredCreatureUid", o.FiredCreatureUid.ToString());
		dictionary.Add("StackDurationRuleType", o.StackDurationRuleType.ToString());
		dictionary.Add("DurationMs", o.DurationMs.ToString());
		dictionary.Add("GlobalTimestamp", o.GlobalTimestamp.ToString());
		return dictionary;
	}
}
