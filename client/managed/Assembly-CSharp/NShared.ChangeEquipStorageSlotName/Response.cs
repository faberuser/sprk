namespace NShared.ChangeEquipStorageSlotName;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int EquipStorageSlotIndex { get; set; }

	public string NewName { get; set; }
}
