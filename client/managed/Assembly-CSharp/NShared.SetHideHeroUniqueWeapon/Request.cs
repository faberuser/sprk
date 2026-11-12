namespace NShared.SetHideHeroUniqueWeapon;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/set_hide_hero_unique_weapon";

	public int HeroIndex { get; set; }

	public byte HideUniqueWeapon { get; set; }

	public int WeaponCpstumeIndex { get; set; }
}
