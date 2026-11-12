namespace NShared.PutEventStep;

[Response]
public class Response : GenericResponse<ResultType>
{
	public EventStepInfo EventStepInfo { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }
}
