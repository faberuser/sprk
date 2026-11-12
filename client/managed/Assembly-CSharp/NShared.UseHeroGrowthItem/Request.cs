namespace NShared.UseHeroGrowthItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/use_hero_growth_item";

	public int ItemIndex { get; set; }

	public int HeroIndex { get; set; }

	public bool GetOtherReward { get; set; }
}
