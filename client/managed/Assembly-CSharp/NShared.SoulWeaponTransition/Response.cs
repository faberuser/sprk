namespace NShared.SoulWeaponTransition;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public SoulWeaponInfo ResultSoulWeaponInfo { get; set; }
}
