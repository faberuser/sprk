using System;

namespace NShared.NOperationRuntime;

public class ModDamage : BaseOperationRuntime, IDamageModifier, IBaseDamageModifier, ISnapshotObject
{
	protected readonly SnapshotObjectMember<int> _modifyCountHolder;

	public ModDamage()
	{
		_modifyCountHolder = new SnapshotObjectMember<int>(this, 0);
	}

	protected override void OnInit()
	{
		int valueInt = base.OperationInfo.GetValueInt(0);
		_modifyCountHolder.Set(valueInt);
	}

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

	bool IDamageModifier.Modify(BattleCreature attacker, ref long amount, ref DamageFlag damageFlag)
	{
		if ((damageFlag & DamageFlag.DerivedDamage) != DamageFlag.None)
		{
			return false;
		}
		int valueInt = base.OperationInfo.GetValueInt(1);
		DamageFlag valueFlag = base.OperationInfo.GetValueFlag<DamageFlag>(2);
		DamageFlag valueFlag2 = base.OperationInfo.GetValueFlag<DamageFlag>(3);
		if (valueInt != 0)
		{
			amount += amount * valueInt / 1000;
			amount = Math.Max(amount, 0L);
		}
		if (valueFlag != DamageFlag.None)
		{
			damageFlag |= valueFlag;
		}
		if (valueFlag2 != DamageFlag.None)
		{
			damageFlag &= ~valueFlag;
		}
		int data = Math.Max(_modifyCountHolder.Get() - 1, 0);
		_modifyCountHolder.Set(data);
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
		return _modifyCountHolder.Get() > 0;
	}
}
