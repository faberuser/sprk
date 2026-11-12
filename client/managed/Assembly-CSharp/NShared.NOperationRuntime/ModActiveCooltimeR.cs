namespace NShared.NOperationRuntime;

public class ModActiveCooltimeR : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		int valueInt = base.OperationInfo.GetValueInt(0);
		int valueInt2 = base.OperationInfo.GetValueInt(1);
		ModActiveCooltime.ActiveSkillFlag valueFlag = base.OperationInfo.GetValueFlag<ModActiveCooltime.ActiveSkillFlag>(2);
		int valueInt3 = base.OperationInfo.GetValueInt(3);
		BattleCreature creature = base.OperationInfo.Creature;
		if (creature != null)
		{
			if (valueInt3 == 0)
			{
				SetCoolTimeByActiveSkillCount(creature, valueInt, valueFlag, valueInt2);
			}
			else
			{
				SetCoolTimeByTargetSlotNo(creature, valueInt3, valueFlag, valueInt2);
			}
		}
	}

	private void SetCoolTimeByActiveSkillCount(BattleCreature creature, int activeSkillCount, ModActiveCooltime.ActiveSkillFlag flag, int ratioM)
	{
		ModActiveCooltime.ForEachActiveSkillIndex(creature, activeSkillCount, flag, delegate(int skillIndex)
		{
			ModActiveCooltime.SetCoolTimeByRatio(creature, skillIndex, ratioM);
		});
	}

	private void SetCoolTimeByTargetSlotNo(BattleCreature creature, int slotNo, ModActiveCooltime.ActiveSkillFlag flag, int ratioM)
	{
		int skillIndexBySlot = creature.CreatureData.GetSkillIndexBySlot(slotNo);
		if (ModActiveCooltime.CheckActiveSkillFlag(creature, flag, skillIndexBySlot))
		{
			ModActiveCooltime.SetCoolTimeByRatio(creature, skillIndexBySlot, ratioM);
		}
		int[] linkedSkillIndexBySlot = creature.CreatureData.GetLinkedSkillIndexBySlot(slotNo);
		if (linkedSkillIndexBySlot == null || linkedSkillIndexBySlot.Length == 0)
		{
			return;
		}
		foreach (int skillIndex in linkedSkillIndexBySlot)
		{
			if (ModActiveCooltime.CheckActiveSkillFlag(creature, flag, skillIndex))
			{
				ModActiveCooltime.SetCoolTimeByRatio(creature, skillIndex, ratioM);
			}
		}
	}
}
