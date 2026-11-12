using System;

namespace NShared.NOperationRuntime;

public class IgnoreDamageUnderHpR : BaseOperationRuntime, IDamageModifier, IBaseDamageModifier, ISnapshotObject
{
	protected readonly SnapshotObjectMember<int> _limitRatioHolder;

	public IgnoreDamageUnderHpR()
	{
		_limitRatioHolder = new SnapshotObjectMember<int>(this, 0);
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		_limitRatioHolder.Set(base.OperationInfo.GetValueInt(0));
		base.OperationInfo.Creature.StatController.AddDamageModifier(this);
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		base.OperationInfo.Creature.StatController.RemoveDamageModifier(this);
	}

	private bool IsUnderHpLimit(int hpRatioM)
	{
		if (_limitRatioHolder.Get() > 0 && hpRatioM <= _limitRatioHolder.Get())
		{
			return true;
		}
		return false;
	}

	bool IDamageModifier.Modify(BattleCreature attacker, ref long amount, ref DamageFlag damageFlag)
	{
		long maxHp = base.OperationInfo.Creature.StatController.GetMaxHp();
		if (maxHp <= 0)
		{
			return false;
		}
		long hp = base.OperationInfo.Creature.StatController.Hp;
		int hpRatioM = (int)(Math.Max(hp - amount, 0L) * 1000 / maxHp);
		if (!IsUnderHpLimit(hpRatioM))
		{
			return false;
		}
		long num = maxHp * _limitRatioHolder.Get() / 1000;
		amount = Math.Max(hp - num, 0L);
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
