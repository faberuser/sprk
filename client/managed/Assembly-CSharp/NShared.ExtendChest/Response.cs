namespace NShared.ExtendChest;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public int NewChestExtend { get; set; }
}
