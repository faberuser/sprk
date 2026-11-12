namespace NShared.SoulWeaponTransitionTicket;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo ItemResult { get; set; }

	public SoulWeaponInfo SourceSoulWeaponInfo { get; set; }

	public SoulWeaponInfo TargetSoulWeaponInfo { get; set; }
}
