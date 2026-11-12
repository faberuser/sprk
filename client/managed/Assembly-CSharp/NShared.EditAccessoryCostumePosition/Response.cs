namespace NShared.EditAccessoryCostumePosition;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PlayerAccessoryCostumeInfo[] PlayerAccessoryCostumeInfos { get; set; }
}
