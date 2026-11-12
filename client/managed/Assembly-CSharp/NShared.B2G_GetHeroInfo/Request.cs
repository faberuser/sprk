namespace NShared.B2G_GetHeroInfo;

[BattlePacket]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/b2g_get_hero_info";

	public long AccountId { get; set; }

	public int[] HeroIndices { get; set; }

	public int[] AiHeroIndices { get; set; }

	public int[] GroupHeroIndices { get; set; }

	public bool IsMaster { get; set; }
}
