namespace NShared;

[JsonSerializable(Javascript = true)]
public class GachaItemResultInfo
{
	public ItemResultInfo ItemResult { get; set; }

	public EquipItemInfo EquipItemResult { get; set; }

	public PetInfo PetItemResult { get; set; }

	public StaminaResultInfo StaminaResult { get; set; }
}
