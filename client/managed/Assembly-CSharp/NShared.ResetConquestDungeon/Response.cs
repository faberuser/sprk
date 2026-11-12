namespace NShared.ResetConquestDungeon;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GemResultInfo GemResult { get; set; }

	public ConquestDungeonInfo ConquestDungeonInfo { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }
}
