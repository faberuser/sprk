namespace NShared.SearchUser;

[Response]
public class Response : GenericResponse<ResultType>
{
	public long Id { get; set; }

	public string Nick { get; set; }

	public int TeamLevel { get; set; }

	public int HeroIndex { get; set; }
}
