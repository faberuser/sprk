namespace NShared;

[JsonSerializable]
public class PlayerEnterBattleLog : IBattleLogRuntime
{
	public BattlePlayerCreateInfo PlayerCreateInfo { get; set; }

	public BattleCreatureCreateInfo[] CreatureCreateInfos { get; set; }

	public ResultCreatureInfo[] InitialCreatureInfos { get; set; }

	public BattleCreatureCreateInfo[] BattleCheckCreatureCreateInfos { get; set; }

	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.PlayerEnter;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		BattlePlayer battlePlayer = battleInstance.AddPlayer(PlayerCreateInfo);
		if (battlePlayer == null)
		{
			return;
		}
		battleInstance.SetInitialCreatureInfo(InitialCreatureInfos);
		if (CreatureCreateInfos != null)
		{
			BattleCreatureCreateInfo[] creatureCreateInfos = CreatureCreateInfos;
			foreach (BattleCreatureCreateInfo createInfo in creatureCreateInfos)
			{
				battleInstance.CreatureContainer.Create(battlePlayer.Id, createInfo);
			}
		}
	}
}
