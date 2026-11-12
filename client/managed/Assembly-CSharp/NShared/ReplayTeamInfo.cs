namespace NShared;

[JsonSerializable(Javascript = true)]
public class ReplayTeamInfo
{
	public long AccountId { get; set; }

	public int Level { get; set; }

	public string NickName { get; set; }

	public int AvaterIndex { get; set; }

	public int MatchScore { get; set; }

	public int TierIndex { get; set; }

	public int TierRank { get; set; }

	public TeamType TeamType { get; set; }

	public ReplayTeamHeroInfo[] HeroInfos { get; set; }
}
