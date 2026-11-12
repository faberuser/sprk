using NShared.NUtil;

namespace NShared.NOperationRuntime;

public class Down : SetStateBase
{
	protected override void OnBegin()
	{
		base.OnBegin();
		if (base.OperationInfo.Creature.StateController.HasModifier(this))
		{
			int valueInt = base.OperationInfo.GetValueInt(0);
			long uid = base.OperationInfo.Uid;
			ProcessDown(base.OperationInfo.FiredCreature, base.OperationInfo.Creature, valueInt, uid);
		}
	}

	public static bool ProcessDown(BattleCreature attackerCreature, BattleCreature damagedCreature, int downDistanceMm, long operationUid)
	{
		if (damagedCreature == null || attackerCreature == null)
		{
			return false;
		}
		Vector3 position = damagedCreature.TransformController.Position;
		Vector3 position2 = attackerCreature.TransformController.Position;
		Vector3 vector = position - position2;
		if (vector.approximatelyZero)
		{
			vector = attackerCreature.TransformController.Direction;
		}
		Vector3 offset = vector.normalized * downDistanceMm / 1000L;
		if (offset.approximatelyZero)
		{
			offset = new Vector3(1L, 0L, 0L);
		}
		damagedCreature.TransformController.MoveBy(offset, sendPacket: true, SetPosition.SetPositionMethodType.Down, operationUid);
		return true;
	}
}
