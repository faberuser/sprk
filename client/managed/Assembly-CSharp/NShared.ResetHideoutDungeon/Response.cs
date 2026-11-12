namespace NShared.ResetHideoutDungeon;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GemResultInfo GemResult { get; set; }

	public HideoutDungeonInfo HideoutDungeonInfo { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }
}
