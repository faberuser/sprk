namespace NShared.GetStamina;

[Response]
public class Response : GenericResponse<ResultType>
{
	public StaminaResultInfo StaminaResult { get; set; }
}
