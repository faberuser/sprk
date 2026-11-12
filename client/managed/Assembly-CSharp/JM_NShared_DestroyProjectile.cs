using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_DestroyProjectile
{
	public static string EncodeJsonString(DestroyProjectile o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(DestroyProjectile o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ProjectileUid", o.ProjectileUid.ToString());
		dictionary.Add("GlobalTimestamp", o.GlobalTimestamp.ToString());
		return dictionary;
	}
}
