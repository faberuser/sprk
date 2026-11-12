using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RecvRewardPhaseStep;

public static class JM_NShared_RecvRewardPhaseStep_Request
{
	public static NShared.RecvRewardPhaseStep.Request Parse(string s)
	{
		return Parse(s, new NShared.RecvRewardPhaseStep.Request());
	}

	public static NShared.RecvRewardPhaseStep.Request Parse(string s, NShared.RecvRewardPhaseStep.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RecvRewardPhaseStep.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RecvRewardPhaseStep.Request());
	}

	public static NShared.RecvRewardPhaseStep.Request Parse(IDictionary d, NShared.RecvRewardPhaseStep.Request o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.Type = BaseJsonMarshaler.ParseValueString(d, "Type");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RecvRewardPhaseStep.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RecvRewardPhaseStep.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		if (o.Type != null)
		{
			dictionary.Add("Type", o.Type);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
