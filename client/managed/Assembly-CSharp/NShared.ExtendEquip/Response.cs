namespace NShared.ExtendEquip;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public int NewInventoryExtend { get; set; }
}
