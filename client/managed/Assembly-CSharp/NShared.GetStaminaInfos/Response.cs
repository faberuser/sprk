namespace NShared.GetStaminaInfos;

[Response]
public class Response : GenericResponse<ResultType>
{
	public StaminaResultInfo[] StaminaResults { get; set; }
}
