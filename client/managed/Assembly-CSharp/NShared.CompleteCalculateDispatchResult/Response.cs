namespace NShared.CompleteCalculateDispatchResult;

[Response]
public class Response : GenericResponse<ResultType>
{
	public DispatchBattleInfo DispatchInfo { get; set; }
}
