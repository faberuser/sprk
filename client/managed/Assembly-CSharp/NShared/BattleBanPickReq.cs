namespace NShared;

[BattlePacket]
public class BattleBanPickReq : BaseRequest
{
	public MatchBanPickState BanPickState { get; set; }

	public int[] HeroIndices { get; set; }

	public bool IsPenalty { get; set; }
}
