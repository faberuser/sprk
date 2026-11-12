namespace NShared;

[JsonSerializable]
public class CreatureReserveOnDungeonSkillBattleLog : IBattleLogRuntime
{
	public TeamType TeamType { get; set; }

	public int SkillSlotIndex { get; set; }

	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.CreatureReserveOnDungeonSkill;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		BattleCreature environmentCreatureOfTeam = battleInstance.GetEnvironmentCreatureOfTeam(TeamType);
		if (environmentCreatureOfTeam == null)
		{
			return;
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(environmentCreatureOfTeam.CreatureIndex);
		if (data != null && environmentCreatureOfTeam.DungeonSkillController.HasDungeonSkill())
		{
			int skillIndexBySlot = data.GetSkillIndexBySlot(SkillSlotIndex);
			SkillData data2 = GameRoot.Instance.SkillDataContainer.GetData(skillIndexBySlot);
			if (data2 != null)
			{
				environmentCreatureOfTeam.SkillController.ReserveDungeonSkill(data2, SkillSlotIndex);
			}
		}
	}
}
