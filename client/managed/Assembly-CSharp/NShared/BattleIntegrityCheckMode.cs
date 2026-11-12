namespace NShared;

[JsonSerializable(Javascript = true)]
public enum BattleIntegrityCheckMode
{
	RemainHp,
	TotalDamage,
	OfflineMatch,
	AliveCount
}
