using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ShieldEnd
{
	public static string EncodeJsonString(ShieldEnd o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ShieldEnd o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("OperationUid", o.OperationUid.ToString());
		dictionary.Add("GlobalTimestamp", o.GlobalTimestamp.ToString());
		return dictionary;
	}
}
