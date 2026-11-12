using System;

namespace NShared.NOperationRuntime;

public class ModAttackByRace : BaseOperationRuntime, IDamageAmplifier, IDamageModifier, IBaseDamageModifier, ISnapshotObject, IAttackModifier
{
	protected readonly SnapshotObjectMember<SnapshotArray<CreatureRaceType>> _raceTypesHolder;

	protected readonly SnapshotObjectMember<int> _addAttackRatioHolder;

	protected readonly SnapshotObjectMember<int> _addDamageRatioHolder;

	protected readonly SnapshotObjectMember<bool> _reverseCheckHolder;

	public ModAttackByRace()
	{
		_raceTypesHolder = new SnapshotObjectMember<SnapshotArray<CreatureRaceType>>(this);
		_addAttackRatioHolder = new SnapshotObjectMember<int>(this, 0);
		_addDamageRatioHolder = new SnapshotObjectMember<int>(this, 0);
		_reverseCheckHolder = new SnapshotObjectMember<bool>(this, value: false);
	}

	protected override void OnInit()
	{
		_raceTypesHolder.Set(base.OperationInfo.GetValueEnums<CreatureRaceType>(0));
		_addAttackRatioHolder.Set(base.OperationInfo.GetValueInt(1));
		_addDamageRatioHolder.Set(base.OperationInfo.GetValueInt(2));
		_reverseCheckHolder.Set(base.OperationInfo.GetValueBool(3));
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		if (_addAttackRatioHolder.Get() != 0)
		{
			base.OperationInfo.Creature.StatController.AddAttackModifier(this);
		}
		if (_addDamageRatioHolder.Get() != 0)
		{
			base.OperationInfo.Creature.StatController.AddDamageModifier(this);
		}
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		base.OperationInfo.Creature.StatController.RemoveAttackModifier(this);
		base.OperationInfo.Creature.StatController.RemoveDamageModifier(this);
	}

	private bool CheckRace(BattleCreature creature)
	{
		if (creature == null || creature.CreatureData == null)
		{
			return false;
		}
		CreatureRaceType[] array = _raceTypesHolder.GET();
		if (array == null)
		{
			return false;
		}
		if (_reverseCheckHolder.Get())
		{
			if (array.Contains(creature.CreatureData.Race))
			{
				return false;
			}
		}
		else if (!array.Contains(creature.CreatureData.Race))
		{
			return false;
		}
		return true;
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

	DamageModifiedType IAttackModifier.GetModifiedType()
	{
		return DamageModifiedType.DamageAmplify;
	}

	bool IAttackModifier.AccumulateAmplifyAttack(BattleCreature damaged, SkillData firedSkillData, ref DamageFlag damageFlag, ref int multPower, ref int addPower)
	{
		if (!CheckRace(damaged))
		{
			return false;
		}
		if ((damageFlag & DamageFlag.IgnoreModAttackByRace) != DamageFlag.None)
		{
			return false;
		}
		int num = _addAttackRatioHolder.Get();
		if (num != 0)
		{
			int num2 = Math.Max(base.OperationInfo.StackCount, 1);
			multPower += num * num2;
			return true;
		}
		return false;
	}

	bool IDamageModifier.Modify(BattleCreature attacker, ref long amount, ref DamageFlag damageFlag)
	{
		if ((damageFlag & DamageFlag.DerivedDamage) != DamageFlag.None)
		{
			return false;
		}
		if (!CheckRace(attacker))
		{
			return false;
		}
		if ((damageFlag & DamageFlag.IgnoreModAttackByRace) != DamageFlag.None)
		{
			return false;
		}
		int num = _addDamageRatioHolder.Get();
		if (num != 0)
		{
			int num2 = Math.Max(base.OperationInfo.StackCount, 1);
			amount = Math.Max(amount * (1000 + num * num2) / 1000, 0L);
		}
		return true;
	}

	bool IDamageAmplifier.AccumulateAmplifyDamage(BattleCreature attacker, ref DamageFlag damageFlag, ref int multPower, ref int addPower)
	{
		if ((damageFlag & DamageFlag.DerivedDamage) != DamageFlag.None)
		{
			return false;
		}
		if (!CheckRace(attacker))
		{
			return false;
		}
		if ((damageFlag & DamageFlag.IgnoreModAttackByRace) != DamageFlag.None)
		{
			return false;
		}
		int num = _addDamageRatioHolder.Get();
		if (num != 0)
		{
			int num2 = Math.Max(base.OperationInfo.StackCount, 1);
			multPower = num * num2;
			return true;
		}
		return false;
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
		return true;
	}
}
