namespace NShared.SoulWeaponRenewOption;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/soul_weapon_renew_option";

	public int EquipItemSlotIndex { get; set; }

	public int CouponItemIndex { get; set; }

	public long OptionStat1 { get; set; }

	public long OptionStat2 { get; set; }

	public long BonusOptionStat1 { get; set; }

	public long BonusOptionStat2 { get; set; }
}
