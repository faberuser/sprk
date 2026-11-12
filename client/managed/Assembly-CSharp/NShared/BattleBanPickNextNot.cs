namespace NShared;

[BattlePacket]
public class BattleBanPickNextNot
{
	public int RemainTimeSec { get; set; }

	public MatchBanPickState BanPickState { get; set; }

	public int[] HeroIndices { get; set; }

	public BanPickAccountInfo[] BanPickAccountInfo { get; set; }
}
