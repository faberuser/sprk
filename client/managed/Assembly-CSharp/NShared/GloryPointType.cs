namespace NShared;

[JsonSerializable(Javascript = true)]
public enum GloryPointType
{
	None,
	[GloryPointType(MatchRewardType = MatchRewardType.Attendance)]
	Attendance,
	[GloryPointType(MatchRewardType = MatchRewardType.Recommand)]
	Recommand,
	[GloryPointType(MatchRewardType = MatchRewardType.Win)]
	Win,
	[GloryPointType(MatchRewardType = MatchRewardType.SuccessiveWin)]
	SuccessiveWin,
	[GloryPointType(MatchRewardType = MatchRewardType.Weekly)]
	Weekly,
	[GloryPointType(MatchRewardType = MatchRewardType.Season)]
	Season
}
