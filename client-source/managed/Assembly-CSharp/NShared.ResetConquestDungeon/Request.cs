namespace NShared.ResetConquestDungeon;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "campaign/reset_conquest_dungeon";

	public int ChapterIndex { get; set; }
}
