namespace NShared.VisitDungeon;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "campaign/visit_dungeon";

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }
}
