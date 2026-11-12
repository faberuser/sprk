using NShared.NUtil;
using NVespa;

namespace NShared.NOperationRuntime;

public class Knockback : SetStateBase
{
	public enum KnockbackDirectionType
	{
		Radial,
		View
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		if (base.OperationInfo.Creature.StateController.HasModifier(this))
		{
			int valueInt = base.OperationInfo.GetValueInt(0);
			KnockbackDirectionType valueEnum = base.OperationInfo.GetValueEnum<KnockbackDirectionType>(1);
			long uid = base.OperationInfo.Uid;
			ProcessKnockback(base.OperationInfo.FiredCreature, base.OperationInfo.Creature, valueInt, valueEnum, uid);
		}
	}

	public static bool ProcessKnockback(BattleCreature attackerCreature, BattleCreature damagedCreature, int knockbackDistanceMm, KnockbackDirectionType knockbackDirectionType, long operationUid)
	{
		if (damagedCreature == null || attackerCreature == null)
		{
			return false;
		}
		Vector3 position = damagedCreature.TransformController.Position;
		Vector3 position2 = attackerCreature.TransformController.Position;
		Vector3 zero = Vector3.zero;
		switch (knockbackDirectionType)
		{
		case KnockbackDirectionType.Radial:
			zero = position - position2;
			break;
		case KnockbackDirectionType.View:
			zero = attackerCreature?.TransformController.Direction ?? Vector3.xAxis;
			break;
		default:
			VespaDebug.LogError("Not implemented type : " + knockbackDirectionType);
			return false;
		}
		if (zero.approximatelyZero)
		{
			zero = attackerCreature.TransformController.Direction;
		}
		Vector3 offset = zero.normalized * knockbackDistanceMm / 1000L;
		damagedCreature.TransformController.MoveBy(offset, sendPacket: true, SetPosition.SetPositionMethodType.Knockback, operationUid);
		return true;
	}
}
