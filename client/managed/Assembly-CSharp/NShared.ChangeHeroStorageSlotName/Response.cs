namespace NShared.ChangeHeroStorageSlotName;

[Response]
public class Response : GenericResponse<ResultType>
{
	public string StorageKey { get; set; }

	public string NewName { get; set; }
}
