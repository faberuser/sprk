using System;

namespace NShared.NOperationRuntime;

public class PropagateDamage : BaseOperationRuntime, IDamageModifier, IBaseDamageModifier, ISnapshotObject
{
	protected readonly SnapshotObjectMember<int> _defendRatioHolder;

	protected readonly SnapshotObjectMember<int> _propagateRatioHolder;

	protected readonly SnapshotObjectMember<DamageFlag> _damageFlagHolder;

	protected readonly SnapshotObjectMember<DamageFlag> _checkFlagHolder;

	protected readonly SnapshotObjectMember<DamagePipelinePosition> _pipelinePositionHolder;

	public PropagateDamage()
	{
		_defendRatioHolder = new SnapshotObjectMember<int>(this, 0);
		_propagateRatioHolder = new SnapshotObjectMember<int>(this, 0);
		_damageFlagHolder = new SnapshotObjectMember<DamageFlag>(this, DamageFlag.None);
		_checkFlagHolder = new SnapshotObjectMember<DamageFlag>(this, DamageFlag.None);
		_pipelinePositionHolder = new SnapshotObjectMember<DamagePipelinePosition>(this, DamagePipelinePosition.Beginning);
	}

	protected override void OnInit()
	{
		_defendRatioHolder.Set(base.OperationInfo.GetValueInt(0));
		_propagateRatioHolder.Set(base.OperationInfo.GetValueInt(1));
		_damageFlagHolder.Set(base.OperationInfo.GetValueFlag<DamageFlag>(2));
		_checkFlagHolder.Set(base.OperationInfo.GetValueFlag<DamageFlag>(3));
		_pipelinePositionHolder.Set(base.OperationInfo.GetValueEnum<DamagePipelinePosition>(4));
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
		if ((damageFlag & DamageFlag.Reflected) != DamageFlag.None)
		{
			return false;
		}
		if (_checkFlagHolder.Get() != DamageFlag.None && (_checkFlagHolder.Get() & damageFlag) == 0)
		{
			return false;
		}
		BattleCreature firedCreature = base.OperationInfo.FiredCreature;
		if (firedCreature == null || !firedCreature.IsAliveAndPlaying())
		{
			return false;
		}
		long num = amount * _defendRatioHolder.Get() / 1000;
		if (num > 0 && attacker != null)
		{
			DamageFlag addedFlag = _damageFlagHolder.Get();
			DamageFlag damageFlag2 = damageFlag.CreateReflectedDamageFlag(addedFlag);
			long amount2 = num * _propagateRatioHolder.Get() / 1000;
			long finalDamage = 0L;
			firedCreature.StatController.GiveDamage(amount2, attacker, base.OperationInfo, base.OperationInfo.Uid, base.OperationInfo.OperationNo, base.OperationInfo.SkillData, base.OperationInfo.FiredSkillData, 1, 1, null, ref damageFlag2, out finalDamage);
		}
		amount = Math.Max(amount - num, 0L);
		return true;
	}

	DamageModifiedType IDamageModifier.GetModifiedType()
	{
		return DamageModifiedType.Reflected;
	}

	DamagePipelinePosition IDamageModifier.GetPipelinePosition()
	{
		return _pipelinePositionHolder.Get();
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
