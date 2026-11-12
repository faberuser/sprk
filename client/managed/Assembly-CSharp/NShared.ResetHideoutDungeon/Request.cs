namespace NShared.ResetHideoutDungeon;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "campaign/reset_hideout_dungeon";

	public int ChapterIndex { get; set; }
}
