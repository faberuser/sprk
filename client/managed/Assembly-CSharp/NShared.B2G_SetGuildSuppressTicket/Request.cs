namespace NShared.B2G_SetGuildSuppressTicket;

[BattlePacket]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/b2g_set_guild_suppress_ticket";

	public long AccountId { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public int DungeonDifficulty { get; set; }
}
