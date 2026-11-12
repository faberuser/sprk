using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_SpawnProjectile
{
	public static string EncodeJsonString(SpawnProjectile o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(SpawnProjectile o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ProjectileUid", o.ProjectileUid.ToString());
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("SkillIndex", o.SkillIndex.ToString());
		dictionary.Add("ProjectileIndex", o.ProjectileIndex.ToString());
		dictionary.Add("FollowTargetUid", o.FollowTargetUid.ToString());
		dictionary.Add("Position", JM_NShared_NUtil_Vector3.EncodeJsonObject(o.Position));
		dictionary.Add("Direction", JM_NShared_NUtil_Vector3.EncodeJsonObject(o.Direction));
		dictionary.Add("GlobalTimestamp", o.GlobalTimestamp.ToString());
		return dictionary;
	}
}
