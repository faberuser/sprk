namespace NShared.GetGuildArenaDeck;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_arena/get_guild_arena_deck";

	public int SeasonIndex { get; set; }

	public long MemberId { get; set; }

	public string EnemyServerGroup { get; set; }

	public long EnemyGuildId { get; set; }

	public long EnemyAccountId { get; set; }
}
