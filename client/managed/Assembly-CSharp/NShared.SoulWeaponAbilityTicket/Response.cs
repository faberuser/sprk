namespace NShared.SoulWeaponAbilityTicket;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo ItemResult { get; set; }

	public ItemResultInfo[] ItemRefundsResults { get; set; }

	public SoulWeaponInfo TargetSoulWeaponInfo { get; set; }
}
