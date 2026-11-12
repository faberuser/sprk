namespace NShared.GetRecommendDeckList;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "recommend_deck/get_recommend_deck_list";

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public int Difficulty { get; set; }
}
