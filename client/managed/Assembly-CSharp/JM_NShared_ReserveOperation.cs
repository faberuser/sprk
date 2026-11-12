using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ReserveOperation
{
	public static string EncodeJsonString(ReserveOperation o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ReserveOperation o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("OperationUid", o.OperationUid.ToString());
		dictionary.Add("FixedDelayTimeMs", o.FixedDelayTimeMs.ToString());
		dictionary.Add("DurationTimeMs", o.DurationTimeMs.ToString());
		dictionary.Add("GlobalTimestamp", o.GlobalTimestamp.ToString());
		return dictionary;
	}
}
