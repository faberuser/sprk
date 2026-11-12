namespace NShared.GetGuildSuppressGroupRankerList;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildSuppressGroupRankerInfo[] GuildSuppressGroupRankerInfos { get; set; }
}
