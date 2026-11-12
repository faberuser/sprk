namespace NShared.ResetEventDungeon;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "campaign/reset_event_dungeon";

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }
}
