namespace NShared;

[JsonSerializable]
public enum WinDeciderType
{
	None,
	DefeatAllEnemy,
	MoreCreatureAlive,
	LowerBossHp,
	Survival,
	Defense
}
