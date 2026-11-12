namespace NShared;

[JsonSerializable(Javascript = true)]
public enum AchievementType
{
	None,
	General,
	Daily,
	DailyEvent,
	Hero,
	Weekly,
	Raid,
	EventOnce,
	Limited,
	PayShop,
	SoulWeapon
}
