using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ChangeMp
{
	public static string EncodeJsonString(ChangeMp o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ChangeMp o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("OperationUid", o.OperationUid.ToString());
		dictionary.Add("Reason", o.Reason.ToString());
		dictionary.Add("Amount", o.Amount.ToString());
		dictionary.Add("FinalMp", o.FinalMp.ToString());
		dictionary.Add("GlobalTimestamp", o.GlobalTimestamp.ToString());
		return dictionary;
	}
}
