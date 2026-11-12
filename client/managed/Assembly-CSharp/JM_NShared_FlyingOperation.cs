using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_FlyingOperation
{
	public static string EncodeJsonString(FlyingOperation o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(FlyingOperation o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("OperationUid", o.OperationUid.ToString());
		dictionary.Add("ReserveDurationTimeMs", o.ReserveDurationTimeMs.ToString());
		dictionary.Add("FlyingDurationTimeMs", o.FlyingDurationTimeMs.ToString());
		dictionary.Add("TargetCreatureUid", o.TargetCreatureUid.ToString());
		dictionary.Add("GlobalTimestamp", o.GlobalTimestamp.ToString());
		return dictionary;
	}
}
