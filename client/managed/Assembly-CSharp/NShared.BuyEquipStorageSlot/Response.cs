namespace NShared.BuyEquipStorageSlot;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public EquipStorageSlotInfo NewEquipStorageSlotInfo { get; set; }

	public EquipPresetInfo EquipPresetInfo { get; set; }
}
