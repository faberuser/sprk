using NShared.NUtil;
using NVespa;

namespace NShared.NOperationRuntime;

public class Pulling : SetStateBase
{
	public enum PullingCenterType
	{
		FiredCreature,
		FiredPosition
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		if (!base.OperationInfo.Creature.StateController.HasModifier(this))
		{
			return;
		}
		int valueInt = base.OperationInfo.GetValueInt(0);
		int valueInt2 = base.OperationInfo.GetValueInt(1);
		int valueInt3 = base.OperationInfo.GetValueInt(2);
		bool valueBool = base.OperationInfo.GetValueBool(3);
		PullingCenterType valueEnum = base.OperationInfo.GetValueEnum<PullingCenterType>(4);
		long uid = base.OperationInfo.Uid;
		BattleCreature firedCreature = base.OperationInfo.FiredCreature;
		if (firedCreature != null)
		{
			Vector3 zero = Vector3.zero;
			switch (valueEnum)
			{
			case PullingCenterType.FiredCreature:
				zero = firedCreature.TransformController.Position;
				break;
			case PullingCenterType.FiredPosition:
				zero = base.OperationInfo.FiredPosition;
				break;
			default:
				VespaDebug.LogError("Unknown center type: " + valueEnum);
				zero = firedCreature.TransformController.Position;
				break;
			}
			ProcessPulling(firedCreature, base.OperationInfo.Creature, valueInt, zero, valueInt2, valueInt3, valueBool, uid);
		}
	}

	public static bool ProcessPulling(BattleCreature attackerCreature, BattleCreature damagedCreature, int distanceMm, Vector3 centerPosition, int centerOffsetMm, int centerRadiusMm, bool allowBackward, long operationUid)
	{
		if (damagedCreature == null || attackerCreature == null)
		{
			return false;
		}
		Vector3 position = damagedCreature.TransformController.Position;
		_ = damagedCreature.CreatureData.SizeMm;
		Vector3 direction = attackerCreature.TransformController.Direction;
		if (centerOffsetMm != 0)
		{
			centerPosition += direction * centerOffsetMm / 1000L;
		}
		Vector3 vector = centerPosition - position;
		int num = (int)vector.Magnitude;
		vector = ((num != 0) ? vector.normalized : (-direction));
		int num2 = distanceMm;
		int num3 = num - num2;
		if (num3 < centerRadiusMm)
		{
			num2 -= centerRadiusMm - num3;
		}
		Vector3 vector2 = vector * num2 / 1000L;
		if (!allowBackward && Vector3.DotProduct(vector2, direction) > 0)
		{
			return false;
		}
		damagedCreature.TransformController.MoveBy(vector2, sendPacket: true, SetPosition.SetPositionMethodType.Pulling, operationUid);
		return true;
	}
}
