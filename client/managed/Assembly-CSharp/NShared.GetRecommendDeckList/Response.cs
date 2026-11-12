namespace NShared.GetRecommendDeckList;

[Response]
public class Response : GenericResponse<ResultType>
{
	public RecommendDeckInfo[] RecommendDecks { get; set; }
}
