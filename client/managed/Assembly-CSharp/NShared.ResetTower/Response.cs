namespace NShared.ResetTower;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3[] CurrencyResults2 { get; set; }

	public TowerInfo TowerInfo { get; set; }
}
