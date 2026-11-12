namespace NShared.GetPunishmentRaidInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public StaminaResultInfo StaminaResult { get; set; }

	public OpenPunishmentRaidInfo OpenPunishmentRaidInfo { get; set; }

	public PunishmentRaidInfo[] PunishmentRaidInfos { get; set; }
}
