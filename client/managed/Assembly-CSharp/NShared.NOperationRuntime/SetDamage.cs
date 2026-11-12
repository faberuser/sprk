using System;

namespace NShared.NOperationRuntime;

public class SetDamage : BaseOperationRuntime, IDamageModifier, IBaseDamageModifier, ISnapshotObject
{
	protected override void OnBegin()
	{
		base.OnBegin();
		base.OperationInfo.Creature.StatController.AddDamageModifier(this);
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		base.OperationInfo.Creature.StatController.RemoveDamageModifier(this);
	}

	public override int GetAddStateCount()
	{
		int valueInt = base.OperationInfo.GetValueInt(0);
		if (valueInt <= 0)
		{
			return int.MaxValue;
		}
		return valueInt;
	}

	bool IDamageModifier.Modify(BattleCreature attacker, ref long amount, ref DamageFlag damageFlag)
	{
		int stackCount = base.OperationInfo.StackCount;
		if (stackCount <= 0)
		{
			return false;
		}
		int valueInt = base.OperationInfo.GetValueInt(1);
		DamageFlag valueFlag = base.OperationInfo.GetValueFlag<DamageFlag>(2);
		DamageFlag valueFlag2 = base.OperationInfo.GetValueFlag<DamageFlag>(3);
		amount = valueInt;
		damageFlag |= DamageFlag.Modified;
		if (valueFlag != DamageFlag.None)
		{
			damageFlag |= valueFlag;
		}
		if (valueFlag2 != DamageFlag.None)
		{
			damageFlag &= ~valueFlag;
		}
		int stackCount2 = Math.Max(stackCount - 1, 0);
		SetStackCount(stackCount2);
		base.OperationInfo.Creature.StateController.OnStateChanged(this);
		return true;
	}

	DamageModifiedType IDamageModifier.GetModifiedType()
	{
		return DamageModifiedType.Else;
	}

	DamagePipelinePosition IDamageModifier.GetPipelinePosition()
	{
		return DamagePipelinePosition.Final;
	}

	long IBaseDamageModifier.GetOperationUid()
	{
		return base.OperationInfo.Uid;
	}

	bool IBaseDamageModifier.IsValid()
	{
		return base.OperationInfo.StackCount > 0;
	}
}
