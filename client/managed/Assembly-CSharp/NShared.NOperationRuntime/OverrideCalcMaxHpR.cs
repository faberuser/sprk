using System;

namespace NShared.NOperationRuntime;

public class OverrideCalcMaxHpR : BaseOperationRuntime, IStatOverrider, ISnapshotObject
{
	protected readonly SnapshotObjectMember<long> _overrideMaxHpHolder;

	protected readonly SnapshotObjectMember<long> _recoverHpHolder;

	public OverrideCalcMaxHpR()
	{
		_overrideMaxHpHolder = new SnapshotObjectMember<long>(this, 0L);
		_recoverHpHolder = new SnapshotObjectMember<long>(this, 0L);
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		BattleCreature creature = base.OperationInfo.Creature;
		if (creature == null)
		{
			_overrideMaxHpHolder.Set(0L);
			return;
		}
		int valueInt = base.OperationInfo.GetValueInt(0);
		long finalStat = creature.StatController.GetFinalStat(StatType.MaxHp);
		long num = finalStat + finalStat * valueInt / 1000;
		_overrideMaxHpHolder.Set(num);
		long hp = creature.StatController.Hp;
		if (hp > num)
		{
			long data = hp - num;
			_recoverHpHolder.Set(data);
			creature.StatController.SetHp(num);
		}
		if (_overrideMaxHpHolder.Get() > 0)
		{
			base.OperationInfo.Creature.StatController.AddOverrider(this);
		}
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		if (_overrideMaxHpHolder.Get() <= 0)
		{
			return;
		}
		base.OperationInfo.Creature.StatController.RemoveOverrider(this);
		if (_recoverHpHolder.Get() > 0)
		{
			BattleCreature creature = base.OperationInfo.Creature;
			if (creature != null)
			{
				long num = _recoverHpHolder.Get();
				long val = creature.StatController.Hp + num;
				long finalStat = creature.StatController.GetFinalStat(StatType.MaxHp);
				val = Math.Min(val, finalStat);
				creature.StatController.SetHp(val);
			}
		}
	}

	StatType IStatOverrider.GetStatType()
	{
		return StatType.MaxHp;
	}

	long IStatOverrider.GetOverridenStatValue()
	{
		return _overrideMaxHpHolder.Get();
	}
}
