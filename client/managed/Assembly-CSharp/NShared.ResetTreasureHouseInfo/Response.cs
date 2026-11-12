namespace NShared.ResetTreasureHouseInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public PlayerTreasureHouseInfo PlayerTreasureHouseInfo { get; set; }
}
