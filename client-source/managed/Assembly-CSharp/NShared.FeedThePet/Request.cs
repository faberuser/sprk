namespace NShared.FeedThePet;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "pet/feed_the_pet";

	public int PetIndex { get; set; }

	public int FeedCount { get; set; }
}
