namespace NShared.B2G_SetMatchResult;

[BattlePacket]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/b2g_set_match_result";

	public int SeasonIndex { get; set; }

	public long MatchUid { get; set; }

	public long AccountId { get; set; }

	public int Win { get; set; }

	public int AvgFightingPower { get; set; }

	public int PlayTime { get; set; }

	public int[] AliveHeroIndices { get; set; }

	public int[] EnterHeroIndices { get; set; }

	public int[] BanHeroIndices { get; set; }

	public MatchArenaType ArenaType { get; set; }

	public bool IsLogon { get; set; }

	public bool IsGiveUp { get; set; }

	public int PenaltyCount { get; set; }

	public int MatchBanPickState { get; set; }
}
