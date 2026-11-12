using System;
using System.Collections.Generic;

namespace NShared.NOperationRuntime;

public class ModAttackAmount : BaseOperationRuntime, IAttackModifier, IBaseDamageModifier, ISnapshotObject
{
	private readonly SnapshotObjectMember<SnapshotArray<int>> _operationValuesHolder;

	public ModAttackAmount()
	{
		_operationValuesHolder = new SnapshotObjectMember<SnapshotArray<int>>(this);
	}

	protected override void OnInit()
	{
		int num = 0;
		List<int> list = new List<int>();
		int num2 = 0;
		while (true)
		{
			int valueInt = base.OperationInfo.GetValueInt(num2);
			if (valueInt == 0)
			{
				break;
			}
			int valueInt2 = base.OperationInfo.GetValueInt(num2 + 1);
			list.Add(valueInt);
			list.Add(valueInt2);
			num += valueInt;
			num2 += 2;
		}
		if (num < 1000)
		{
			list.Add(1000 - num);
			list.Add(0);
		}
		_operationValuesHolder.Set(list.ToArray());
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
		int[] array = _operationValuesHolder.GET();
		if (array == null || array.Length == 0)
		{
			return false;
		}
		int num = base.OperationInfo.Creature.NextRandom(1000, "ModAttack");
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < array.Length / 2; i += 2)
		{
			int num4 = array[i * 2];
			int num5 = array[i * 2 + 1];
			num2 += num4;
			if (num < num2)
			{
				num3 = num5;
				break;
			}
		}
		if (num3 != 0)
		{
			amount = Math.Max(amount * (1000 + num3) / 1000, 0L);
		}
		return true;
	}

	DamageModifiedType IAttackModifier.GetModifiedType()
	{
		return DamageModifiedType.AttackAmplify;
	}

	bool IAttackModifier.AccumulateAmplifyAttack(BattleCreature damaged, SkillData firedSkillData, ref DamageFlag damageFlag, ref int multPower, ref int addPower)
	{
		int[] array = _operationValuesHolder.GET();
		if (array == null || array.Length == 0)
		{
			return false;
		}
		int num = base.OperationInfo.Creature.NextRandom(1000, "ModAttack");
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < array.Length / 2; i += 2)
		{
			int num4 = array[i * 2];
			int num5 = array[i * 2 + 1];
			num2 += num4;
			if (num < num2)
			{
				num3 = num5;
				break;
			}
		}
		multPower += num3;
		return true;
	}

	long IBaseDamageModifier.GetOperationUid()
	{
		return base.OperationInfo.Uid;
	}

	bool IBaseDamageModifier.IsValid()
	{
		return true;
	}
}
