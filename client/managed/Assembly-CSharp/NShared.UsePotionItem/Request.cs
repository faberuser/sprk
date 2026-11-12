namespace NShared.UsePotionItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/use_potion_item";

	public int ItemIndex { get; set; }

	public int HeroIndex { get; set; }

	public int ItemCount { get; set; }

	public int ChapterIndex { get; set; }
}
