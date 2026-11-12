namespace NShared.NRaid;

[JsonSerializable(Javascript = true)]
public class ChallengeRaidInfo
{
	public int SeasonIndex { get; set; }

	public int ChallengeRaidIndex { get; set; }

	public int RaidIndex { get; set; }

	public string BeginTime { get; set; }

	public string EndTime { get; set; }

	public RaidRewardType raidRewardType { get; set; }
}
