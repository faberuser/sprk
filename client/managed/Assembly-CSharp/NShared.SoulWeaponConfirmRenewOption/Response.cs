namespace NShared.SoulWeaponConfirmRenewOption;

[Response]
public class Response : GenericResponse<ResultType>
{
	public SoulWeaponInfo ResultSoulWeaponInfo { get; set; }
}
