namespace NShared.BookmarkHero;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/bookmark_hero";

	public int[] HeroIndex { get; set; }
}
