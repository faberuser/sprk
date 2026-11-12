namespace NShared.BuyPurchaseDungeon;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public PlayerItemTimeDurationInfo[] ItemTimeDurations { get; set; }
}
