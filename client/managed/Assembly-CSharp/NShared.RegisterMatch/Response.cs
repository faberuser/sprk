namespace NShared.RegisterMatch;

[Response]
public class Response : GenericResponse<ResultType>
{
	public MatchedAccountInfo AccountInfo { get; set; }

	public StaminaResultInfo SwordResult { get; set; }

	public DeckInfo DeckInfo { get; set; }
}
