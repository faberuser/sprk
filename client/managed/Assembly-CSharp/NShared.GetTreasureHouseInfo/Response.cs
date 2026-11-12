namespace NShared.GetTreasureHouseInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public PlayerTreasureHouseInfo PlayerTreasureHouseInfo { get; set; }
}
