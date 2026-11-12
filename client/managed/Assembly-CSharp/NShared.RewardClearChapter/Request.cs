namespace NShared.RewardClearChapter;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "campaign/reward_clear_chapter";

	public int ChapterIndex { get; set; }

	public int Step { get; set; }
}
