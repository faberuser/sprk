namespace NShared.ResetPunishmentRaid;

[Response]
public class Response : GenericResponse<ResultType>
{
	public OpenPunishmentRaidInfo OpenPunishmentRaidInfo { get; set; }

	public PunishmentRaidInfo[] PunishmentRaidInfos { get; set; }
}
