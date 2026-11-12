namespace NShared;

[JsonSerializable(Javascript = true)]
public class MatchResultInfo
{
	public long MatchUid { get; set; }

	public MatchArenaType ArenaType { get; set; }

	public bool Win { get; set; }

	public bool GiveUp { get; set; }

	public bool Canceled { get; set; }

	public int GainedMatchScore { get; set; }

	public int NewMatchScore { get; set; }

	public int NewTierIndex { get; set; }

	public int NewTierRank { get; set; }

	public int NewTotalRank { get; set; }

	public int PrevTierIndex { get; set; }

	public int PrevTierRank { get; set; }

	public int PrevTotalRank { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public HeroExpResultInfo[] HeroExpResults { get; set; }

	public StaminaResultInfo SwordResult { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }
}
