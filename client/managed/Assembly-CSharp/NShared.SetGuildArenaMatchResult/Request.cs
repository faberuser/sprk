namespace NShared.SetGuildArenaMatchResult;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_arena/set_guild_arena_match_result";

	public int Win { get; set; }

	public int PlayTime { get; set; }

	public int[] AliveHeroIndices { get; set; }

	public string BattleLogString { get; set; }

	public string CreatureInfoString { get; set; }
}
