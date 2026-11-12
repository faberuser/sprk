namespace NShared.BuyCostume;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public HeroCostumeResultInfo HeroCostumeResult { get; set; }

	public CostumeInfo[] CostumeResults { get; set; }
}
