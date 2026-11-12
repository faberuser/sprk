namespace NShared.GetGuildArenaRanker;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildArenaRankerInfo MyRankerInfo { get; set; }

	public GuildArenaRankerInfo[] Rankers { get; set; }
}
