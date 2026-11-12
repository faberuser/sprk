using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ResistOperation
{
	public static string EncodeJsonString(ResistOperation o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ResistOperation o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("OperationUid", o.OperationUid.ToString());
		if (o.StateType != null)
		{
			dictionary.Add("StateType", o.StateType);
		}
		dictionary.Add("GlobalTimestamp", o.GlobalTimestamp.ToString());
		return dictionary;
	}
}
