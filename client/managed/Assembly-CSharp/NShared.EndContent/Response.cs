namespace NShared.EndContent;

[Response]
public class Response : GenericResponse<ResultType>
{
	public string ForwardHost { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ChapterDungeonInfo[] CampaignResults { get; set; }

	public StaminaResultInfo StaminaResult { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo[] EquipItemInfos { get; set; }

	public PunishmentRaidInfo[] PunishmentRaidInfos { get; set; }

	public OpenPunishmentRaidInfo[] OpenPunishmentRaidInfos { get; set; }
}
