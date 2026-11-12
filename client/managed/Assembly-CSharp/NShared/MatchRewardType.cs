namespace NShared;

[JsonSerializable(Javascript = true)]
public enum MatchRewardType
{
	None,
	Attendance,
	Win,
	Recommand,
	SuccessiveWin,
	Weekly,
	Season
}
