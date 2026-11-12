using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BeginMove
{
	public static string EncodeJsonString(BeginMove o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BeginMove o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("StartPosition", JM_NShared_NUtil_Vector3.EncodeJsonObject(o.StartPosition));
		dictionary.Add("TargetPosition", JM_NShared_NUtil_Vector3.EncodeJsonObject(o.TargetPosition));
		dictionary.Add("TargetOffsetIndex", o.TargetOffsetIndex.ToString());
		dictionary.Add("SpeedMms", o.SpeedMms.ToString());
		dictionary.Add("TargetCreatureUid", o.TargetCreatureUid.ToString());
		dictionary.Add("ApproachDistanceMm", o.ApproachDistanceMm.ToString());
		dictionary.Add("OperationUid", o.OperationUid.ToString());
		dictionary.Add("GlobalTimestamp", o.GlobalTimestamp.ToString());
		return dictionary;
	}
}
