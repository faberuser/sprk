namespace NShared.GetSeasonInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public MatchSeasonData SeasonData { get; set; }
}
