namespace NShared.ResetAllRaid;

[Response]
public class Response : GenericResponse<ResultType>
{
	public RaidResultInfo RaidResult { get; set; }
}
