using System;
using NShared.NUtil;

namespace NShared.NOperationRuntime;

public class SetDirection : BaseOperationRuntime
{
	public enum DirectionType
	{
		Axis,
		Position,
		View
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		switch (base.OperationInfo.GetValueEnum<DirectionType>(0))
		{
		case DirectionType.Axis:
			SetDirection_Axis();
			break;
		case DirectionType.Position:
			SetDirection_Position();
			break;
		case DirectionType.View:
			SetDirection_View();
			break;
		}
	}

	private void SetDirection_Axis()
	{
		string valueString = base.OperationInfo.GetValueString(1);
		int valueInt = base.OperationInfo.GetValueInt(2);
		bool valueBool = base.OperationInfo.GetValueBool(3);
		Vector3 vector = Vector3.xAxis;
		if (string.Equals(valueString, "z", StringComparison.OrdinalIgnoreCase))
		{
			vector = Vector3.zAxis;
		}
		if (base.OperationInfo.Creature.TeamController.TeamType == TeamType.Right)
		{
			vector = -vector;
		}
		if (valueBool)
		{
			vector = -vector;
		}
		if (valueInt != 0)
		{
			vector.Yaw(valueInt * 1000);
		}
		base.OperationInfo.Creature.TransformController.SetDirection(vector, checkStatic: false);
	}

	private void SetDirection_Position()
	{
		int valueInt = base.OperationInfo.GetValueInt(1);
		int valueInt2 = base.OperationInfo.GetValueInt(2);
		int valueInt3 = base.OperationInfo.GetValueInt(3);
		Vector3 vector = new Vector3(valueInt, 0L, valueInt2);
		Vector3 position = base.OperationInfo.Creature.TransformController.Position;
		if (!(position == vector))
		{
			Vector3 direction = vector - position;
			if (valueInt3 != 0)
			{
				direction.Yaw(valueInt3 * 1000);
			}
			base.OperationInfo.Creature.TransformController.SetDirection(direction, checkStatic: false);
		}
	}

	private void SetDirection_View()
	{
		int valueInt = base.OperationInfo.GetValueInt(1);
		bool valueBool = base.OperationInfo.GetValueBool(2);
		Vector3 vector = base.OperationInfo.Creature.TransformController.Direction;
		if (valueBool)
		{
			vector = -vector;
		}
		if (valueInt != 0)
		{
			vector.Yaw(valueInt * 1000);
		}
		base.OperationInfo.Creature.TransformController.SetDirection(vector, checkStatic: false);
	}
}
