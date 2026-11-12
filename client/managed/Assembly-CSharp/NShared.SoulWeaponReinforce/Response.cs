namespace NShared.SoulWeaponReinforce;

[Response]
public class Response : GenericResponse<ResultType>
{
	public bool ReinforceSuccess { get; set; }

	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public SoulWeaponInfo ResultSoulWeaponInfo { get; set; }
}
