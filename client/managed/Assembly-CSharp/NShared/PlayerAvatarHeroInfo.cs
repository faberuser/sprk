namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerAvatarHeroInfo
{
	public long AccountId { get; set; }

	public int HeroIndex { get; set; }

	public int CostumeIndex { get; set; }

	public byte HideUniqueWeapon { get; set; }

	public int WeaponCostumeIndex { get; set; }

	public int HairCostumeIndex { get; set; }

	public int AccessoryCostumeIndex1 { get; set; }

	public string PositionInfo1 { get; set; }

	public int AccessoryCostumeIndex2 { get; set; }

	public string PositionInfo2 { get; set; }

	public int AccessoryCostumeIndex3 { get; set; }

	public string PositionInfo3 { get; set; }

	public int AccessoryCostumeIndex4 { get; set; }

	public string PositionInfo4 { get; set; }

	public int AccessoryCostumeIndex5 { get; set; }

	public string PositionInfo5 { get; set; }

	public int AccessoryCostumeIndex6 { get; set; }

	public string PositionInfo6 { get; set; }
}
