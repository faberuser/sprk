namespace NShared.UseWeaponUniqueSelectItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/use_weapon_unique_select_item";

	public int ItemIndex { get; set; }

	public int WeaponUniqueItemIndex { get; set; }

	public int ItemCount { get; set; }
}
