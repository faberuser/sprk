namespace NShared.ExtendPunishmentRuneStorageSlot;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public int PunishmentRuneStorageExtend { get; set; }
}
