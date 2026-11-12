namespace NShared.GiveUpEclipseDungeon;

[Response]
public class Response : GenericResponse<ResultType>
{
	public EclipseDungeonInfo EclipseDungeonInfo { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }
}
