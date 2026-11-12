namespace NShared.GetGuildRaidList;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildRaidDungeonInfo[] GuildRaidInfos { get; set; }
}
