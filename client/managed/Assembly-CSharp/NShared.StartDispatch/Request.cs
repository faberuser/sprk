namespace NShared.StartDispatch;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "dispatch/start_dispatch";

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public int DeckIndex { get; set; }

	public int Difficulty { get; set; }

	public int[] HeroIndices { get; set; }

	public int RepeatCount { get; set; }
}
