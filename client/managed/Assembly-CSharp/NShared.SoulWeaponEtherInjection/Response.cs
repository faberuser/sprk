namespace NShared.SoulWeaponEtherInjection;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public SoulWeaponInfo ResultSoulWeaponInfo { get; set; }

	public ItemResultInfo[] RemovedItemResultInfos { get; set; }
}
