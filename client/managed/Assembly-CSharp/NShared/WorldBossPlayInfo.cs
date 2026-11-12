namespace NShared;

[JsonSerializable(Javascript = true)]
public class WorldBossPlayInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => Index;

	public int Index { get; set; }

	public int Season { get; set; }

	public int TryCount { get; set; }

	public int ResetCount { get; set; }

	public long MaxDamage { get; set; }

	public long TotalDamage { get; set; }

	public string MaxDamageTime { get; set; }

	public string LastBattleTime { get; set; }

	public string RewardedTime { get; set; }
}
