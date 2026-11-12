namespace NShared.HeroInnResetTime;

[Response]
public class Response : GenericResponse<ResultType>
{
	public string LastResetTime { get; set; }

	public string NextResetTime { get; set; }
}
