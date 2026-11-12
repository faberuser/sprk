using System;

namespace NShared.NOperationRuntime;

public class EscapeDeath : BaseOperationRuntime, IDamageModifier, IBaseDamageModifier, ISnapshotObject
{
	protected readonly SnapshotObjectMember<bool> _healedHolder;

	public EscapeDeath()
	{
		_healedHolder = new SnapshotObjectMember<bool>(this, value: false);
	}

	protected override void OnInit()
	{
		_healedHolder.Set(data: false);
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
		if (_healedHolder.Get())
		{
			return false;
		}
		long hp = base.OperationInfo.Creature.StatController.Hp;
		if (hp - amount > 0)
		{
			return false;
		}
		long num = hp - 1;
		int valueInt = base.OperationInfo.GetValueInt(0);
		int valueInt2 = base.OperationInfo.GetValueInt(1);
		if (valueInt > 0)
		{
			num = Math.Min(hp - valueInt, 0L);
		}
		if (valueInt2 > 0)
		{
			HpShield hpShield = SnapshotObjectPool<HpShield>.Alloc();
			hpShield.Init(valueInt2, 0, DamageFlag.Physical | DamageFlag.Magical, 0L);
			base.OperationInfo.Creature.StatController.AddDamageModifier(hpShield);
		}
		_healedHolder.Set(data: true);
		OnEscapeDeath onEscapeDeath = SnapshotObjectPool<OnEscapeDeath>.Alloc();
		onEscapeDeath.TriggerCreature = attacker;
		onEscapeDeath.TargetCreatures = base.OperationInfo.TargetCreatures;
		base.OperationInfo.Creature.SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onEscapeDeath);
		amount = num;
		return true;
	}

	DamageModifiedType IDamageModifier.GetModifiedType()
	{
		return DamageModifiedType.EscapeDeath;
	}

	DamagePipelinePosition IDamageModifier.GetPipelinePosition()
	{
		return DamagePipelinePosition.EndOfFinal;
	}

	long IBaseDamageModifier.GetOperationUid()
	{
		return base.OperationInfo.Uid;
	}

	bool IBaseDamageModifier.IsValid()
	{
		return !_healedHolder.Get();
	}
}
