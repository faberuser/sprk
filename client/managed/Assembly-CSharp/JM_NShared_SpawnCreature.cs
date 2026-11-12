using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_SpawnCreature
{
	public static string EncodeJsonString(SpawnCreature o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(SpawnCreature o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("PlayerId", o.PlayerId.ToString());
		dictionary.Add("CreateInfo", JM_NShared_BattleCreatureCreateInfo.EncodeJsonObject(o.CreateInfo));
		dictionary.Add("SpawnPosition", JM_NShared_NUtil_Vector3.EncodeJsonObject(o.SpawnPosition));
		dictionary.Add("GlobalTimestamp", o.GlobalTimestamp.ToString());
		return dictionary;
	}
}
