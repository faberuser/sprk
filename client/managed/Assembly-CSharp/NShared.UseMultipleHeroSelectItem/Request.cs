namespace NShared.UseMultipleHeroSelectItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/use_multiple_hero_select_item";

	public int ItemIndex { get; set; }

	public int[] LeftHeroIndices { get; set; }

	public int[] RightHeroIndices { get; set; }

	public bool LeftAllOwnedReward { get; set; }

	public bool RightAllOwnedReward { get; set; }
}
