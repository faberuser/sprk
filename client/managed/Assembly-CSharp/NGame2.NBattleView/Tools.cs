using UnityEngine;

namespace NGame2.NBattleView;

public static class Tools
{
	public static VectorXZ GetDirection(int creatureUid, int targetUid)
	{
		if (!BattleViewGenericSingleton<CreatureManager>.IsValidInstance)
		{
			return VectorXZ.right;
		}
		Creature creature = BattleViewGenericSingleton<CreatureManager>.instance.GetCreature(creatureUid);
		if (creature == null)
		{
			return VectorXZ.right;
		}
		Creature creature2 = BattleViewGenericSingleton<CreatureManager>.instance.GetCreature(targetUid);
		if (creature2 != null)
		{
			VectorXZ vectorXZ = Vespa_Unity_Vector3_Extension_Method.VecXZ(creature2.Position);
			VectorXZ vectorXZ2 = Vespa_Unity_Vector3_Extension_Method.VecXZ(creature.Position);
			VectorXZ vectorXZ3 = vectorXZ - vectorXZ2;
			if (vectorXZ3.magnitude > Mathf.Epsilon)
			{
				return vectorXZ3.normalized;
			}
		}
		return creature.Action.GetDirection();
	}
}
