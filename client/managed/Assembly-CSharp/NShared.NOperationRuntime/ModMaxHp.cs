using System;

namespace NShared.NOperationRuntime;

public class ModMaxHp : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MaxHp };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}

	protected override void OnBegin()
	{
		HandleMaxHpChanged(this, delegate
		{
			base.OnBegin();
		});
	}

	protected override void OnStatUpdatedAndMakeDirty()
	{
		HandleMaxHpChanged(this, base.MakeCreatureStatDirty);
	}

	public static void HandleMaxHpChanged(SetStatBase setStat, Action changeAction)
	{
		if (setStat == null)
		{
			return;
		}
		OperationInfo operationInfo = setStat.OperationInfo;
		if (operationInfo == null)
		{
			return;
		}
		BattleCreature creature = operationInfo.Creature;
		if (creature != null)
		{
			long maxHp = creature.StatController.GetMaxHp();
			long hp = creature.StatController.Hp;
			changeAction?.Invoke();
			long maxHp2 = creature.StatController.GetMaxHp();
			long num = ((hp >= maxHp) ? maxHp2 : ((long)((decimal)hp * ((decimal)maxHp2 / (decimal)maxHp))));
			if (num <= 0)
			{
				num = 1L;
			}
			creature.StatController.SetHp(num);
		}
		else
		{
			changeAction?.Invoke();
		}
	}
}
