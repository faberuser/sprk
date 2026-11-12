namespace NShared;

[JsonSerializable]
public class CreatureReserveOnSkillBattleLog : IBattleLogRuntime
{
	public int CreatureUid { get; set; }

	public int SkillSlotIndex { get; set; }

	public int ManualTargetUid { get; set; }

	public bool AutoReserved { get; set; }

	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.CreatureReserveOnSkill;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
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
		if (SkillSlotIndex == 10)
		{
			if (creature.SoulWeaponSkillController.EquippedSoulWeapon)
			{
				SkillData basicSkillData = creature.SoulWeaponSkillController.BasicSkillData;
				creature.SkillController.ReserveSoulWeaponSkill(basicSkillData, SkillSlotIndex, ManualTargetUid, AutoReserved);
			}
			return;
		}
		int skillIndexBySlot = data.GetSkillIndexBySlot(SkillSlotIndex);
		SkillData data2 = GameRoot.Instance.SkillDataContainer.GetData(skillIndexBySlot);
		if (data2 != null)
		{
			creature.SkillController.ReserveSkill(data2, SkillSlotIndex, ManualTargetUid, AutoReserved);
		}
	}
}
