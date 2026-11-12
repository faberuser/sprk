namespace NShared.NOperationRuntime;

public class CreateShieldR : BaseOperationRuntime, IDamageModifier, IBaseDamageModifier, ISnapshotObject, IShield
{
	protected readonly SnapshotObjectMember<HpShield> _hpShield;

	public HpShield HpShield => _hpShield.Get();

	public CreateShieldR()
	{
		_hpShield = new SnapshotObjectMember<HpShield>(this);
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		long shieldHp = 0L;
		int shieldCount = 0;
		DamageFlag damageFlag = DamageFlag.None;
		GetShieldHp(base.OperationInfo, out shieldHp, out shieldCount, out damageFlag);
		BattleCreature firedCreature = base.OperationInfo.FiredCreature;
		if (firedCreature != null)
		{
			int modifiedInt = firedCreature.SkillController.ModifierContainer.GetModifiedInt(base.OperationInfo.SkillIndex, base.OperationInfo.OperationNo, OperationFieldType.SkillPowerR, 0, 0);
			int modifiedInt2 = firedCreature.SkillController.ModifierContainer.GetModifiedInt(base.OperationInfo.SkillIndex, base.OperationInfo.OperationNo, OperationFieldType.SkillPower, 0, 0);
			shieldHp = StatController.MultAndAddStat(shieldHp, modifiedInt, modifiedInt2);
			int num = (int)firedCreature.StatController.GetFinalStat(StatType.ShieldPowerR);
			shieldHp = StatController.MultAndAddStat(shieldHp, num, 0L);
		}
		HpShield hpShield = SnapshotObjectPool<HpShield>.Alloc();
		hpShield.Init(shieldHp, shieldCount, damageFlag, 0L);
		_hpShield.Set(hpShield);
		base.OperationInfo.Creature.StatController.AddDamageModifier(this);
		ShieldBegin shieldBegin = new ShieldBegin();
		shieldBegin.CreatureUid = base.OperationInfo.Creature.Uid;
		shieldBegin.OperationUid = base.OperationInfo.Uid;
		shieldBegin.DamageFlag = hpShield.DamageFlag;
		shieldBegin.ShieldMaxHp = hpShield.ShieldMaxHp;
		shieldBegin.StateType = base.OperationInfo.State;
		base.OperationInfo.Creature.RaiseBattleEvent(shieldBegin);
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		base.OperationInfo.Creature.StatController.RemoveDamageModifier(this);
		ShieldEnd shieldEnd = new ShieldEnd();
		shieldEnd.CreatureUid = base.OperationInfo.Creature.Uid;
		shieldEnd.OperationUid = base.OperationInfo.Uid;
		base.OperationInfo.Creature.RaiseBattleEvent(shieldEnd);
	}

	protected virtual void GetShieldHp(OperationInfo operationInfo, out long shieldHp, out int shieldCount, out DamageFlag damageFlag)
	{
		int valueInt = operationInfo.GetValueInt(0);
		long valueLong = operationInfo.GetValueLong(1, 0L);
		shieldHp = operationInfo.FiredCreature.StatController.GetHealAmountByPower(valueInt) + valueLong;
		shieldCount = operationInfo.GetValueInt(2);
		damageFlag = operationInfo.GetValueFlag<DamageFlag>(3);
	}

	bool IDamageModifier.Modify(BattleCreature attacker, ref long amount, ref DamageFlag damageFlag)
	{
		bool num = HpShield.Modify(attacker, ref amount, ref damageFlag);
		if (!HpShield.IsValid())
		{
			base.OperationInfo.Expired = true;
		}
		if (num)
		{
			ShieldModified shieldModified = new ShieldModified();
			shieldModified.CreatureUid = base.OperationInfo.Creature.Uid;
			shieldModified.OperationUid = base.OperationInfo.Uid;
			shieldModified.ShieldHp = HpShield.ShieldHp;
			shieldModified.ShieldMaxHp = HpShield.ShieldMaxHp;
			base.OperationInfo.Creature.RaiseBattleEvent(shieldModified);
		}
		return true;
	}

	DamageModifiedType IDamageModifier.GetModifiedType()
	{
		return DamageModifiedType.Shield;
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
		return HpShield.IsValid();
	}

	long IShield.GetShieldHp()
	{
		return HpShield.ShieldHp;
	}
}
