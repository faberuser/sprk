namespace NShared.SoulWeaponUpgrade;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public SoulWeaponInfo ResultSoulWeaponInfo { get; set; }

	public ItemResultInfo RemovedItemResultInfo { get; set; }
}
