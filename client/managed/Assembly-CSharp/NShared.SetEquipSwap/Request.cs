namespace NShared.SetEquipSwap;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/set_equip_swap";

	public int HeroIndex { get; set; }

	public int[] HeroPartIndex { get; set; }

	public int[] EquipItemSlotIndex { get; set; }
}
