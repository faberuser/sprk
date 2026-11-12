namespace NShared.NOperationRuntime;

public class ReflectDamage : BaseOperationRuntime, IDamageModifier, IBaseDamageModifier, ISnapshotObject
{
	protected readonly SnapshotObjectMember<int> _occurRatioHolder;

	protected readonly SnapshotObjectMember<int> _attackRatioHolder;

	protected readonly SnapshotObjectMember<int> _damageRatioHolder;

	protected readonly SnapshotObjectMember<DamageFlag> _damageFlagHolder;

	public ReflectDamage()
	{
		_occurRatioHolder = new SnapshotObjectMember<int>(this, 0);
		_attackRatioHolder = new SnapshotObjectMember<int>(this, 0);
		_damageRatioHolder = new SnapshotObjectMember<int>(this, 0);
		_damageFlagHolder = new SnapshotObjectMember<DamageFlag>(this, DamageFlag.None);
	}

	protected override void OnInit()
	{
		_occurRatioHolder.Set(base.OperationInfo.GetValueInt(0));
		_attackRatioHolder.Set(base.OperationInfo.GetValueInt(1));
		_damageRatioHolder.Set(base.OperationInfo.GetValueInt(2));
		_damageFlagHolder.Set(base.OperationInfo.GetValueFlag<DamageFlag>(3));
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
		if (!base.OperationInfo.Creature.TestChanceRateResult(_occurRatioHolder.Get()))
		{
			return false;
		}
		long num = amount * _attackRatioHolder.Get() / 1000;
		if (num > 0 && attacker != null)
		{
			DamageFlag addedFlag = _damageFlagHolder.Get();
			DamageFlag damageFlag2 = damageFlag.CreateReflectedDamageFlag(addedFlag);
			long finalDamage = 0L;
			attacker.StatController.GiveDamage(num, base.OperationInfo.Creature, base.OperationInfo, base.OperationInfo.Uid, base.OperationInfo.OperationNo, base.OperationInfo.SkillData, base.OperationInfo.FiredSkillData, base.OperationInfo.SkillLevel, base.OperationInfo.StackCount, null, ref damageFlag2, out finalDamage);
		}
		long num2 = amount * _damageRatioHolder.Get() / 1000;
		amount = num2;
		return true;
	}

	DamageModifiedType IDamageModifier.GetModifiedType()
	{
		return DamageModifiedType.Reflected;
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
