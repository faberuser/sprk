namespace NShared.UseEquipOptionSelectItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/use_equip_option_select_item";

	public int ItemIndex { get; set; }

	public int EquipOptionItemIndex { get; set; }

	public int[] EquipOptionIndices { get; set; }

	public int[] EquipExtraOptionIndices { get; set; }

	public int ItemCount { get; set; }
}
