namespace NShared.UpgradeEquipTier;

[Response]
public class Response : GenericResponse<ResultType>
{
	public EquipItemInfo ResultEquipItem { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public int EquipHeroIndex { get; set; }

	public EquipStorageSlotInfo NewEquipStorageSlotInfo { get; set; }

	public EquipPresetInfo[] EquipPresetInfos { get; set; }
}
