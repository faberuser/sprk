namespace NShared;

[JsonSerializable]
public class CreatureCreateBattleLog : IBattleLogRuntime
{
	public long PlayerId { get; set; }

	public BattleCreatureCreateInfo CreateInfo { get; set; }

	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.CreatureCreate;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		battleInstance.CreatureContainer.Create(PlayerId, CreateInfo);
	}
}
