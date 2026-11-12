using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_Heal
{
	public static string EncodeJsonString(Heal o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(Heal o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("OperationUid", o.OperationUid.ToString());
		dictionary.Add("Amount", o.Amount.ToString());
		dictionary.Add("FinalHp", o.FinalHp.ToString());
		dictionary.Add("FinalPercentage", o.FinalPercentage.ToString());
		dictionary.Add("HealFlag", o.HealFlag.ToString());
		dictionary.Add("GlobalTimestamp", o.GlobalTimestamp.ToString());
		return dictionary;
	}
}
