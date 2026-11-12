namespace NShared.CompleteScenarioDungeon;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ChapterDungeonInfo DungeonInfo { get; set; }
}
