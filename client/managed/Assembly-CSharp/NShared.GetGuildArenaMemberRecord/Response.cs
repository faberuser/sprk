namespace NShared.GetGuildArenaMemberRecord;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildArenaMemberRecordInfo[] MemberRecords { get; set; }
}
