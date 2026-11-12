namespace NShared.SetGuildArenaDeck;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_arena/set_guild_arena_deck";

	public int SeasonIndex { get; set; }

	public int[] HeroIndices1 { get; set; }

	public int[] HeroIndices2 { get; set; }

	public int[] HeroIndices3 { get; set; }

	public int[] HeroIndices4 { get; set; }

	public int[] HeroIndices5 { get; set; }
}
