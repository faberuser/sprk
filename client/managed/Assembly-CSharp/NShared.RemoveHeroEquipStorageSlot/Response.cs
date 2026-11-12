namespace NShared.RemoveHeroEquipStorageSlot;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int EquipStorageSlotIndex { get; set; }
}
