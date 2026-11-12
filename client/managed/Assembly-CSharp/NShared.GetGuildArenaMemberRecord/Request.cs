namespace NShared.GetGuildArenaMemberRecord;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_arena/get_guild_arena_member_record";

	public int SessionIndex { get; set; }

	public string ServerGroup { get; set; }

	public long GuildId { get; set; }

	public GuildArenaRecordType RecordType { get; set; }
}
