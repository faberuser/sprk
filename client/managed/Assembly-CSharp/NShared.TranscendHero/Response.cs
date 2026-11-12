namespace NShared.TranscendHero;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo ItemResult { get; set; }

	public int NewTranscended { get; set; }

	public ExpResultInfo TeamExpResult { get; set; }

	public StaminaResultInfo StaminaResult { get; set; }
}
