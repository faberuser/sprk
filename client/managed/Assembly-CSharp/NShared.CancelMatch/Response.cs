namespace NShared.CancelMatch;

[Response]
public class Response : GenericResponse<ResultType>
{
	public StaminaResultInfo SwordResult { get; set; }
}
