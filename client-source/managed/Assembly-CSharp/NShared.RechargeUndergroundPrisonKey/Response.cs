namespace NShared.RechargeUndergroundPrisonKey;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public StaminaResultInfo UndergroundPrisonKeyResult { get; set; }
}
