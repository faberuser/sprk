using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ShieldBegin
{
	public static string EncodeJsonString(ShieldBegin o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ShieldBegin o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("OperationUid", o.OperationUid.ToString());
		dictionary.Add("DamageFlag", o.DamageFlag.ToString());
		dictionary.Add("ShieldMaxHp", o.ShieldMaxHp.ToString());
		if (o.StateType != null)
		{
			dictionary.Add("StateType", o.StateType);
		}
		dictionary.Add("GlobalTimestamp", o.GlobalTimestamp.ToString());
		return dictionary;
	}
}
