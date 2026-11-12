namespace NShared.RechargeStamina;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public StaminaResultInfo StaminaResult { get; set; }
}
