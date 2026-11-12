namespace NShared.GetEclipseDeck;

[Response]
public class Response : GenericResponse<ResultType>
{
	public EclipseDeckResult[] DeckResults { get; set; }
}
