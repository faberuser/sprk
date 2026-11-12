namespace NShared.ResetEventDungeon;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ChapterDungeonInfo DungeonInfo { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public EventDungeonPointResultInfo EventDungeonPointResult { get; set; }

	public EventDungeonPoint2ResultInfo EventDungeonPoint2Result { get; set; }
}
