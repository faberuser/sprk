namespace NShared.GetGuildArenaRankerRecord;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildArenaRankerRecordInfo[] RankerRecords { get; set; }
}
