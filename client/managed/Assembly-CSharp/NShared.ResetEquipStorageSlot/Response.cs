namespace NShared.ResetEquipStorageSlot;

[Response]
public class Response : GenericResponse<ResultType>
{
	public EquipStorageSlotInfo EquipStorageSlotInfo { get; set; }

	public EquipPresetInfo EquipPresetInfo { get; set; }
}
