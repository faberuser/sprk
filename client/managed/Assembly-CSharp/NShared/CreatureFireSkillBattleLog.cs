namespace NShared;

[JsonSerializable]
public class CreatureFireSkillBattleLog : IBattleLogRuntime
{
	public int CreatureUid { get; set; }

	public int SkillSlotIndex { get; set; }

	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.CreatureFireSkill;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		if (!battleInstance.IsBattleEnabled())
		{
			return;
		}
		BattleCreature creature = battleInstance.CreatureContainer.GetCreature(CreatureUid);
		if (creature == null)
		{
			return;
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(creature.CreatureIndex);
		if (data == null)
		{
			return;
		}
		int skillIndexBySlot = data.GetSkillIndexBySlot(SkillSlotIndex);
		SkillData data2 = GameRoot.Instance.SkillDataContainer.GetData(skillIndexBySlot);
		if (data2 != null)
		{
			if (SkillSlotIndex != 10)
			{
				creature.SkillController.UseSkill(data2, new SkillUseOption(), null, 0, isActiveSkill: false, SkillSlotIndex);
			}
			else
			{
				creature.SkillController.UseSkill(data2, SkillUseOption.GetSoulWeaponSkillUseOption(), null, 0, isActiveSkill: false, SkillSlotIndex);
			}
		}
	}
}
