using System.Collections.Generic;
using NShared.NUtil;

namespace NShared.NOperationRuntime;

public class MoveRandom : BaseOperationRuntime
{
	private static int[] _angles;

	protected override void OnBegin()
	{
		base.OnBegin();
		InitRandomAngles();
		int valueInt = base.OperationInfo.GetValueInt(0);
		int valueInt2 = base.OperationInfo.GetValueInt(1);
		int valueInt3 = base.OperationInfo.GetValueInt(2);
		base.OperationInfo.Creature.NextRandom(valueInt, valueInt2);
		int num = base.OperationInfo.Creature.NextRandom(0, _angles.Length);
		int num2 = _angles[num % _angles.Length];
		Vector3 vector = Vector3.Yaw(Vector3.forward, num2 * 1000);
		Vector3 targetPosition = base.OperationInfo.Creature.TransformController.Position + vector;
		base.OperationInfo.Creature.TransformController.MoveToPosition(targetPosition, valueInt3, 0, base.OperationInfo.Uid, isManualMove: false);
	}

	private static void InitRandomAngles()
	{
		if (_angles == null)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < 360; i += 20)
			{
				list.Add(i);
			}
			_angles = list.ToArray();
		}
	}
}
