using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class MatchBanPickTierRewardData : MatchRewardData
{
	[Key(14)]
	public int WinReward { get; set; }

	[Key(15)]
	public int AttendanceReward { get; set; }

	[Key(16)]
	public int WeeklyReward { get; set; }

	[Key(17)]
	public int WeeklyBoxReward { get; set; }

	[Key(18)]
	public string[] ServerBuff { get; set; }

	[Key(19)]
	public int ShowRewardIndex { get; set; }
}
