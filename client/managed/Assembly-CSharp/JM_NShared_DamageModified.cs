using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_DamageModified
{
	public static string EncodeJsonString(DamageModified o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(DamageModified o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("OperationUid", o.OperationUid.ToString());
		dictionary.Add("PipelinePosition", o.PipelinePosition.ToString());
		dictionary.Add("Mult", o.Mult.ToString());
		dictionary.Add("Add", o.Add.ToString());
		dictionary.Add("BeforeDamage", o.BeforeDamage.ToString());
		dictionary.Add("AfterDamage", o.AfterDamage.ToString());
		dictionary.Add("ModifiedType", o.ModifiedType.ToString());
		dictionary.Add("GlobalTimestamp", o.GlobalTimestamp.ToString());
		return dictionary;
	}
}
