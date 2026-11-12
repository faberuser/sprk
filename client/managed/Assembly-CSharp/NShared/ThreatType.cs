namespace NShared;

[JsonSerializable(Javascript = true)]
public enum ThreatType
{
	None,
	Damage,
	CriticalDamage,
	Heal,
	OverHeal
}
