namespace NShared.RemoveHeroStorageSlot;

[Response]
public class Response : GenericResponse<ResultType>
{
	public string StorageKey { get; set; }
}
