using System;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class AttendanceData
{
	public int Index { get; set; }

	public string StartDate { get; set; }

	public string EndDate { get; set; }

	public EAttendanceKeyword Keyword { get; set; }

	public string Title { get; set; }

	public byte ManualStart { get; set; }

	public byte RewardEndDay { get; set; }

	public int Repeatable { get; set; }

	public int PageSize { get; set; }

	public int InitialPage { get; set; }

	public int OpenLevel { get; set; }

	public int NextAttendanceIndex { get; set; }

	public string ApplyAttendanceTime { get; set; }

	[Obsolete("[// ## 미사용] 사용하지 않음 (Anniversary1_XXX, Anniversary1Paid_XXX 전용으로만 사용됨)", false)]
	public int GroupIndex { get; set; }

	public AttendanceRewards[] Reward { get; set; }

	public int AttendanceRewardableDays { get; set; }

	public int SendMail { get; set; }
}
