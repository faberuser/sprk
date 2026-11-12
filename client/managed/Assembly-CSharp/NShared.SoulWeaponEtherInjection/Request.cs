namespace NShared.SoulWeaponEtherInjection;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/soul_weapon_ether_injection";

	public int EquipItemSlotIndex { get; set; }

	public int[] InjectionItemInfos { get; set; }
}
