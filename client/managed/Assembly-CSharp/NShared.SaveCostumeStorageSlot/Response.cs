namespace NShared.SaveCostumeStorageSlot;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CostumeStorageSlotInfo CostumeStorageSlotInfo { get; set; }
}
