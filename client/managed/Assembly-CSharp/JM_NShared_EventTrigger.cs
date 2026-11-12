using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EventTrigger
{
	public static string EncodeJsonString(EventTrigger o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EventTrigger o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("Values", BaseJsonMarshaler.EncodeArray(o.Values));
		dictionary.Add("OperationUid", o.OperationUid.ToString());
		dictionary.Add("GlobalTimestamp", o.GlobalTimestamp.ToString());
		return dictionary;
	}
}
