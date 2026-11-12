namespace NGameAsset;

public struct WeaponStock
{
	public string WeaponPrefix;

	public int WeaponStar;

	public bool IsEquipSoulWeapon;

	public int SoulWeaponGrade;

	public WeaponStock(string prefix, int star = 0, bool isEquipSoulWeapon = false, int soulWeaponGrade = 0)
	{
		WeaponPrefix = prefix;
		WeaponStar = star;
		IsEquipSoulWeapon = isEquipSoulWeapon;
		SoulWeaponGrade = soulWeaponGrade;
	}
}
