namespace NShared;

[JsonSerializable(Javascript = true)]
public class HeroCostumeResultInfo
{
	public int HeroIndex { get; set; }

	public int CostumeIndex { get; set; }

	public byte HideUniqueWeapon { get; set; }

	public int WeaponCostumeIndex { get; set; }

	public int HairCostumeIndex { get; set; }

	public int AccessoryCostumeIndex1 { get; set; }

	public int AccessoryCostumeIndex2 { get; set; }

	public int AccessoryCostumeIndex3 { get; set; }

	public int AccessoryCostumeIndex4 { get; set; }

	public int AccessoryCostumeIndex5 { get; set; }

	public int AccessoryCostumeIndex6 { get; set; }
}
