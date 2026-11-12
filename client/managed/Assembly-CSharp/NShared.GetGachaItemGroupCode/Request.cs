namespace NShared.GetGachaItemGroupCode;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip_gacha/get_gacha_item_group_code";

	public int GachaIndex { get; set; }

	public HighGachaCategoryType HighGachaCategory { get; set; }
}
