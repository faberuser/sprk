namespace NShared.NRaid;

[JsonSerializable(Javascript = true)]
public class ChallengeRaidRankerInfo
{
	public int Rank { get; set; }

	public long AccountId { get; set; }

	public string Nick { get; set; }

	public long CurrentScore { get; set; }

	public long Score { get; set; }

	public int CurrentBattleTime { get; set; }

	public int BattleTime { get; set; }

	public int AvatarHeroIndex { get; set; }
}
