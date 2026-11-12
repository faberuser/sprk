namespace NShared.BuyKingsPassLevel;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public PlayerKingsPassExp PlayerKingsPassExpInfo { get; set; }
}
