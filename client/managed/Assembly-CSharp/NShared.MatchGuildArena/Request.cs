namespace NShared.MatchGuildArena;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_arena/match_guild_arena";

	public MatchArenaType ArenaType { get; set; }

	public int[] HeroIndices { get; set; }

	public int SessionIndex { get; set; }

	public string EnemyServerGroup { get; set; }

	public long EnemyGuildId { get; set; }

	public long EnemyAccountId { get; set; }

	public int EnemyDeckIndex { get; set; }
}
