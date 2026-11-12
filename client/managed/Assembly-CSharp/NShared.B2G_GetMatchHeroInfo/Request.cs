namespace NShared.B2G_GetMatchHeroInfo;

[BattlePacket]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/b2g_get_match_hero_info";

	public long AccountId { get; set; }

	public int[] HeroIndices { get; set; }
}
