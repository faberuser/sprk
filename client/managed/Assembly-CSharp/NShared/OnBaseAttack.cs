namespace NShared;

public class OnBaseAttack : DamageTrigger, ITriggerByDamage, ITriggerAmount
{
	private readonly SnapshotObjectMember<long> _initialAmount;

	private readonly SnapshotObjectMember<long> _amountHolder;

	private readonly SnapshotObjectMember<DamageFlag> _damageFlagHolder;

	public override SkillTriggerType TriggerType => SkillTriggerType.OnBaseAttack;

	public long InitialAmount
	{
		get
		{
			return _initialAmount.Get();
		}
		set
		{
			_initialAmount.Set(value);
		}
	}

	public long Amount
	{
		get
		{
			return _amountHolder.Get();
		}
		set
		{
			_amountHolder.Set(value);
		}
	}

	public DamageFlag DamageFlag
	{
		get
		{
			return _damageFlagHolder.Get();
		}
		set
		{
			_damageFlagHolder.Set(value);
		}
	}

	public OnBaseAttack()
	{
		_initialAmount = new SnapshotObjectMember<long>(this, 0L);
		_amountHolder = new SnapshotObjectMember<long>(this, 0L);
		_damageFlagHolder = new SnapshotObjectMember<DamageFlag>(this, DamageFlag.None);
	}

	public override bool CheckTriggerValue(BattleCreature firedCreature, ITriggerValueGetter valueGetter)
	{
		if (!base.CheckTriggerValue(firedCreature, valueGetter))
		{
			return false;
		}
		if (valueGetter.GetTriggerValueT<DamageFlag>(firedCreature, 1, out var value) && value != DamageFlag.None && (value & DamageFlag) == 0)
		{
			return false;
		}
		if (valueGetter.GetTriggerValueT<DamageFlag>(firedCreature, 2, out var value2) && value2 != DamageFlag.None && (value2 & DamageFlag) != DamageFlag.None)
		{
			return false;
		}
		return true;
	}

	DamageFlag ITriggerByDamage.GetFlag()
	{
		return DamageFlag;
	}

	long ITriggerAmount.GetAmount()
	{
		return Amount;
	}

	long ITriggerAmount.GetInitialAmount()
	{
		return InitialAmount;
	}
}
