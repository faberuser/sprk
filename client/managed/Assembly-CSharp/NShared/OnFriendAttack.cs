namespace NShared;

public class OnFriendAttack : DamageTrigger, ITriggerByDamage, ITriggerAmount
{
	private readonly SnapshotObjectMember<long> _initialAmount;

	private readonly SnapshotObjectMember<long> _amountHolder;

	private readonly SnapshotObjectMember<DamageFlag> _damageFlagHolder;

	public override SkillTriggerType TriggerType => SkillTriggerType.OnFriendAttack;

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

	public OnFriendAttack()
	{
		_initialAmount = new SnapshotObjectMember<long>(this, 0L);
		_amountHolder = new SnapshotObjectMember<long>(this, 0L);
		_damageFlagHolder = new SnapshotObjectMember<DamageFlag>(this, DamageFlag.None);
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
