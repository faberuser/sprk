namespace NShared;

public class OnHeal : DamageTrigger, ITriggerByHeal, ITriggerAmount
{
	private readonly SnapshotObjectMember<long> _initialAmount;

	private readonly SnapshotObjectMember<long> _amountHolder;

	private readonly SnapshotObjectMember<long> _finalHpHolder;

	private readonly SnapshotObjectMember<long> _overHeal;

	private readonly SnapshotObjectMember<HealFlag> _healFlagHolder;

	public override SkillTriggerType TriggerType => SkillTriggerType.OnHeal;

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

	public long OverHeal
	{
		get
		{
			return _overHeal.Get();
		}
		set
		{
			_overHeal.Set(value);
		}
	}

	public long FinalHp
	{
		get
		{
			return _finalHpHolder.Get();
		}
		set
		{
			_finalHpHolder.Set(value);
		}
	}

	public HealFlag HealFlag
	{
		get
		{
			return _healFlagHolder.Get();
		}
		set
		{
			_healFlagHolder.Set(value);
		}
	}

	public OnHeal()
	{
		_initialAmount = new SnapshotObjectMember<long>(this, 0L);
		_amountHolder = new SnapshotObjectMember<long>(this, 0L);
		_finalHpHolder = new SnapshotObjectMember<long>(this, 0L);
		_overHeal = new SnapshotObjectMember<long>(this, 0L);
		_healFlagHolder = new SnapshotObjectMember<HealFlag>(this, HealFlag.None);
	}

	HealFlag ITriggerByHeal.GetFlag()
	{
		return HealFlag;
	}

	long ITriggerAmount.GetAmount()
	{
		return Amount;
	}

	long ITriggerAmount.GetInitialAmount()
	{
		return InitialAmount;
	}

	long ITriggerByHeal.GetOverHeal()
	{
		return OverHeal;
	}
}
