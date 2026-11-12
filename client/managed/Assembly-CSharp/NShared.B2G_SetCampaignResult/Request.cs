namespace NShared.B2G_SetCampaignResult;

[BattlePacket]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/b2g_set_campaign_result";

	public long AccountId { get; set; }

	public long[] FinishedAccountIds { get; set; }

	public int PlayTime { get; set; }

	public int ElapsedPlayTime { get; set; }

	public bool Win { get; set; }

	public bool IsMaster { get; set; }

	public int AvgHeroLevel { get; set; }

	public int[] AliveHeroIndices { get; set; }

	public long TotalDamage { get; set; }
}
