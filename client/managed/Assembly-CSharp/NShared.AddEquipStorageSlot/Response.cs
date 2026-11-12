namespace NShared.AddEquipStorageSlot;

[Response]
public class Response : GenericResponse<ResultType>
{
	public EquipStorageSlotInfo OldEquipStorageSlotInfo { get; set; }

	public EquipStorageSlotInfo EquipStorageSlotInfo { get; set; }

	public EquipPresetInfo EquipPresetInfo { get; set; }
}
