using System;

namespace NShared.NOperationRuntime;

public class ModNextBaseAttackR : BaseOperationRuntime, IAttackModifier, IBaseDamageModifier, ISnapshotObject
{
	protected readonly SnapshotObjectMember<int> _addRatioHolder;

	protected readonly SnapshotObjectMember<int> _addAmountHolder;

	protected readonly SnapshotObjectMember<int> _countHolder;

	protected readonly SnapshotObjectMember<bool> _infinityHolder;

	public ModNextBaseAttackR()
	{
		_addRatioHolder = new SnapshotObjectMember<int>(this, 0);
		_addAmountHolder = new SnapshotObjectMember<int>(this, 0);
		_countHolder = new SnapshotObjectMember<int>(this, 0);
		_infinityHolder = new SnapshotObjectMember<bool>(this, value: false);
	}

	protected override void OnInit()
	{
		_addRatioHolder.Set(base.OperationInfo.GetValueInt(0));
		_addAmountHolder.Set(base.OperationInfo.GetValueInt(1));
		int valueInt = base.OperationInfo.GetValueInt(2);
		bool data = valueInt == -1;
		_infinityHolder.Set(data);
		_countHolder.Set((valueInt <= 0) ? 1 : valueInt);
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		base.OperationInfo.Creature.StatController.AddAttackModifier(this);
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		base.OperationInfo.Creature.StatController.RemoveAttackModifier(this);
	}

	bool IAttackModifier.Modify(BattleCreature damaged, SkillData firedSkillData, ref long amount, ref DamageFlag damageFlag)
	{
		int multPower = 0;
		int addPower = 0;
		if (((IAttackModifier)this).AccumulateAmplifyAttack(damaged, firedSkillData, ref damageFlag, ref multPower, ref addPower))
		{
			amount = Math.Max(amount * (1000 + multPower) / 1000, 0L) + addPower;
			return true;
		}
		return false;
	}

	bool IAttackModifier.AccumulateAmplifyAttack(BattleCreature damaged, SkillData firedSkillData, ref DamageFlag damageFlag, ref int multPower, ref int addPower)
	{
		if (firedSkillData == null || !firedSkillData.BaseAttack)
		{
			return false;
		}
		bool flag = _infinityHolder.Get();
		int num = _countHolder.Get();
		if (!flag && num <= 0)
		{
			return false;
		}
		int num2 = _addRatioHolder.Get();
		if (num2 != 0)
		{
			int num3 = ((base.OperationInfo.StackCount <= 1) ? 1 : base.OperationInfo.StackCount);
			multPower += num2 * num3;
		}
		int num4 = _addAmountHolder.Get();
		if (num4 != 0)
		{
			addPower += num4;
		}
		if (!flag)
		{
			_countHolder.Set(num - 1);
		}
		return true;
	}

	DamageModifiedType IAttackModifier.GetModifiedType()
	{
		return DamageModifiedType.AttackAmplify;
	}

	long IBaseDamageModifier.GetOperationUid()
	{
		return base.OperationInfo.Uid;
	}

	bool IBaseDamageModifier.IsValid()
	{
		if (_infinityHolder.Get())
		{
			return true;
		}
		return _countHolder.Get() > 0;
	}
}
