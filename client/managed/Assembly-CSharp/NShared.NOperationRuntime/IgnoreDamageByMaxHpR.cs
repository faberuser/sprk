namespace NShared.NOperationRuntime;

public class IgnoreDamageByMaxHpR : BaseOperationRuntime, IDamageModifier, IBaseDamageModifier, ISnapshotObject
{
	protected readonly SnapshotObjectMember<int> _limitRatioHolder;

	protected readonly SnapshotObjectMember<int> _cooltimeMsHolder;

	protected readonly SnapshotObjectMember<long> _lastIgnoreTimeHolder;

	public IgnoreDamageByMaxHpR()
	{
		_limitRatioHolder = new SnapshotObjectMember<int>(this, 0);
		_cooltimeMsHolder = new SnapshotObjectMember<int>(this, 0);
		_lastIgnoreTimeHolder = new SnapshotObjectMember<long>(this, 0L);
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		_limitRatioHolder.Set(base.OperationInfo.GetValueInt(0));
		_cooltimeMsHolder.Set(base.OperationInfo.GetValueInt(1));
		_lastIgnoreTimeHolder.Set(0L);
		base.OperationInfo.Creature.StatController.AddDamageModifier(this);
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		base.OperationInfo.Creature.StatController.RemoveDamageModifier(this);
	}

	bool IDamageModifier.Modify(BattleCreature attacker, ref long amount, ref DamageFlag damageFlag)
	{
		long currentTimeMs = base.OperationInfo.BattleInstance.MainTimer.GetCurrentTimeMs();
		int num = _cooltimeMsHolder.Get();
		if (num > 0 && currentTimeMs - _lastIgnoreTimeHolder.Get() < num)
		{
			return false;
		}
		long maxHp = base.OperationInfo.Creature.StatController.GetMaxHp();
		if (maxHp <= 0)
		{
			return false;
		}
		long num2 = maxHp * _limitRatioHolder.Get() / 1000;
		if (amount <= num2)
		{
			return false;
		}
		amount = num2;
		_lastIgnoreTimeHolder.Set(currentTimeMs);
		return true;
	}

	DamageModifiedType IDamageModifier.GetModifiedType()
	{
		return DamageModifiedType.None;
	}

	DamagePipelinePosition IDamageModifier.GetPipelinePosition()
	{
		return DamagePipelinePosition.AfterFinal;
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
