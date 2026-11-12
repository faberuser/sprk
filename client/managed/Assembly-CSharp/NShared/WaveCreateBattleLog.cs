namespace NShared;

[JsonSerializable]
public class WaveCreateBattleLog : IBattleLogRuntime
{
	public int RandomSeedByMonth { get; set; }

	public int[] ProjectileGroupIndicesFromServer { get; set; }

	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.WaveCreate;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		battleInstance.SetRandomSeedByMonth(RandomSeedByMonth);
		battleInstance.SetProjectileGroupIndicesFromServer(ProjectileGroupIndicesFromServer);
	}
}
