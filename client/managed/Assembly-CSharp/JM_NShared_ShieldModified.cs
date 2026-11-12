using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ShieldModified
{
	public static string EncodeJsonString(ShieldModified o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ShieldModified o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("OperationUid", o.OperationUid.ToString());
		dictionary.Add("ShieldMaxHp", o.ShieldMaxHp.ToString());
		dictionary.Add("ShieldHp", o.ShieldHp.ToString());
		dictionary.Add("GlobalTimestamp", o.GlobalTimestamp.ToString());
		return dictionary;
	}
}
