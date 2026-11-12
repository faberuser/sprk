namespace NShared;

public class HpShield : SnapshotObject, IDamageModifier, IBaseDamageModifier, ISnapshotObject
{
	public const DamageFlag DefaultDamageFlag = DamageFlag.Physical | DamageFlag.Magical;

	private readonly SnapshotObjectMember<long> _shieldHpHolder;

	private readonly SnapshotObjectMember<long> _shieldMaxHpHolder;

	private readonly SnapshotObjectMember<int> _shieldCountHolder;

	private readonly SnapshotObjectMember<DamageFlag> _damageFlagHolder;

	private readonly SnapshotObjectMember<long> _operationUidHolder;

	public long ShieldHp
	{
		get
		{
			return _shieldHpHolder;
		}
		set
		{
			_shieldHpHolder.Set(value);
		}
	}

	public long ShieldMaxHp
	{
		get
		{
			return _shieldMaxHpHolder;
		}
		set
		{
			_shieldMaxHpHolder.Set(value);
		}
	}

	public int ShieldCount
	{
		get
		{
			return _shieldCountHolder;
		}
		set
		{
			_shieldCountHolder.Set(value);
		}
	}

	public DamageFlag DamageFlag
	{
		get
		{
			return _damageFlagHolder;
		}
		set
		{
			_damageFlagHolder.Set(value);
		}
	}

	public HpShield()
	{
		_shieldHpHolder = new SnapshotObjectMember<long>(this, 0L);
		_shieldMaxHpHolder = new SnapshotObjectMember<long>(this, 0L);
		_shieldCountHolder = new SnapshotObjectMember<int>(this, 0);
		_damageFlagHolder = new SnapshotObjectMember<DamageFlag>(this, DamageFlag.None);
		_operationUidHolder = new SnapshotObjectMember<long>(this, 0L);
	}

	public void Init(long shieldHp, int shieldCount = 0, DamageFlag damageFlag = DamageFlag.Physical | DamageFlag.Magical, long operationUid = 0L)
	{
		_shieldHpHolder.Set(shieldHp);
		_shieldMaxHpHolder.Set(shieldHp);
		_shieldCountHolder.Set(shieldCount);
		_damageFlagHolder.Set((damageFlag == DamageFlag.None) ? (DamageFlag.Physical | DamageFlag.Magical) : damageFlag);
		_operationUidHolder.Set(operationUid);
	}

	public bool Modify(BattleCreature attacker, ref long amount, ref DamageFlag damageFlag)
	{
		if (amount <= 0)
		{
			return false;
		}
		if ((damageFlag & DamageFlag) == 0)
		{
			return false;
		}
		if (ShieldCount > 0)
		{
			ShieldCount--;
			if (ShieldCount <= 0)
			{
				ShieldHp = 0L;
				amount = 0L;
				return true;
			}
		}
		if (ShieldHp > amount)
		{
			ShieldHp -= amount;
			amount = 0L;
			return true;
		}
		amount -= ShieldHp;
		ShieldHp = 0L;
		return true;
	}

	public bool IsValid()
	{
		return ShieldHp > 0;
	}

	long IBaseDamageModifier.GetOperationUid()
	{
		return _operationUidHolder.Get();
	}

	DamageModifiedType IDamageModifier.GetModifiedType()
	{
		return DamageModifiedType.Shield;
	}

	DamagePipelinePosition IDamageModifier.GetPipelinePosition()
	{
		return DamagePipelinePosition.Final;
	}
}
