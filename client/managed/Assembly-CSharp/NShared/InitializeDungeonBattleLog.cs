namespace NShared;

[JsonSerializable]
public class InitializeDungeonBattleLog : IBattleLogRuntime
{
	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public long PlayerId { get; set; }

	public BattleCreatureCreateInfo[] CreateInfos { get; set; }

	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.InitialDungeonBattleLog;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		DestroyAllCreature(battleInstance);
		SetCampaignDungeon(battleInstance);
		CreateCreatures(battleInstance);
	}

	private void DestroyAllCreature(BattleInstance battleInstance)
	{
		TeamType[] array = new TeamType[2]
		{
			TeamType.Left,
			TeamType.Right
		};
		foreach (TeamType teamId in array)
		{
			BattleCreatureList allCreaturesOfTeam = battleInstance.CreatureContainer.GetAllCreaturesOfTeam((int)teamId);
			for (int j = 0; j < allCreaturesOfTeam.Count; j++)
			{
				BattleCreature battleCreature = allCreaturesOfTeam[j];
				battleCreature.OnBeginResting(raiseDestroyEvent: true);
				battleInstance.CreatureContainer.Destroy(battleCreature.Uid, raiseEvent: true);
			}
		}
	}

	private void SetCampaignDungeon(BattleInstance battleInstance)
	{
		int difficulty = battleInstance.Difficulty;
		bool isScenario = battleInstance.IsScenario;
		_ = battleInstance.GameSpeedRatio;
		battleInstance.SetCampaignDungeon(ChapterIndex, DungeonIndex, difficulty, isScenario);
	}

	private void CreateCreatures(BattleInstance battleInstance)
	{
		BattleCreatureCreateInfo[] createInfos = CreateInfos;
		foreach (BattleCreatureCreateInfo createInfo in createInfos)
		{
			battleInstance.CreatureContainer.Create(PlayerId, createInfo);
		}
	}
}
