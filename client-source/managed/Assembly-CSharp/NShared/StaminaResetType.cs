namespace NShared;

[JsonSerializable(Javascript = true)]
public enum StaminaResetType
{
	None,
	Init,
	InitDaily,
	InitMonthly,
	InitDailyHour,
	InitWeekly
}
