namespace NShared.UseRecipeItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/use_recipe_item";

	public int ItemIndex { get; set; }
}
