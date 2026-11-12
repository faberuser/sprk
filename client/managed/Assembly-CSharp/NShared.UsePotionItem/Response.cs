namespace NShared.UsePotionItem;

[Response]
public class Response : GenericResponse<ResultType>
{
	public StaminaResultInfo StaminaResult { get; set; }

	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public ItemResultInfo ItemResult { get; set; }

	public HeroExpResultInfo HeroExpResult { get; set; }

	public HeroTranscendResultInfo HeroTranscendResult { get; set; }

	public PlayerItemTimeDurationInfo[] ItemTimeDurationInfos { get; set; }

	public HideoutDungeonInfo HideoutDungeonInfo { get; set; }

	public ConquestDungeonInfo ConquestDungeonInfo { get; set; }

	public int NewInventoryExtend { get; set; }

	public int NewEquipStoageSlotExtend { get; set; }

	public int NewChestExtend { get; set; }
}
