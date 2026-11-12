namespace NShared;

[JsonSerializable(Javascript = true)]
public enum GuildSkillType
{
	None,
	AttackUpPercent,
	DefenseUpPercent,
	AddGoldPercent,
	AddExpPercent,
	GuildRaidAttackUpPercent,
	GuildRaidDefenseUpPercent,
	GuildRaidMaxHpUpPercent,
	GuildRaidCriticalPowerUpPercent,
	MaxHpUpPercent,
	CriticalPowerUpPercent,
	GuildSuppressAttackUpPercent,
	GuildSuppressDefenseUpPercent,
	GuildSuppressMaxHpUpPercent,
	GuildSuppressCriticalPowerUpPercent,
	End
}
