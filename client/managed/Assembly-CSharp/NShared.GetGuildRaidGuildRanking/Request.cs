namespace NShared.GetGuildRaidGuildRanking;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_raid/get_guild_raid_guild_ranking";

	public int ChapterIndex { get; set; }

	public int Difficulty { get; set; }
}
