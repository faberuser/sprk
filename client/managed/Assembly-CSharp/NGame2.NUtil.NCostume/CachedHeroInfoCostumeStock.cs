using NShared;

namespace NGame2.NUtil.NCostume;

public struct CachedHeroInfoCostumeStock
{
	public static CachedHeroInfoCostumeStock NullStock;

	public int HeroIndex;

	public int CostumeIndex;

	public byte HideUniqueWeapon;

	public int WeaponCostumeIndex;

	public int HairCostumeIndex;

	public int AccessoryCostumeIndex1;

	public int AccessoryCostumeIndex2;

	public int AccessoryCostumeIndex3;

	public int AccessoryCostumeIndex4;

	public int AccessoryCostumeIndex5;

	public int AccessoryCostumeIndex6;

	public PositionInfo AccessoryPositionInfo1;

	public PositionInfo AccessoryPositionInfo2;

	public PositionInfo AccessoryPositionInfo3;

	public PositionInfo AccessoryPositionInfo4;

	public PositionInfo AccessoryPositionInfo5;

	public PositionInfo AccessoryPositionInfo6;

	public bool IsDefaultCostume()
	{
		if (CostumeIndex != 0)
		{
			return false;
		}
		if (HideUniqueWeapon != 0)
		{
			return false;
		}
		if (WeaponCostumeIndex != 0)
		{
			return false;
		}
		if (HairCostumeIndex != 0)
		{
			return false;
		}
		if (AccessoryCostumeIndex1 != 0)
		{
			return false;
		}
		if (AccessoryCostumeIndex2 != 0)
		{
			return false;
		}
		if (AccessoryCostumeIndex3 != 0)
		{
			return false;
		}
		if (AccessoryCostumeIndex4 != 0)
		{
			return false;
		}
		if (AccessoryCostumeIndex5 != 0)
		{
			return false;
		}
		if (AccessoryCostumeIndex6 != 0)
		{
			return false;
		}
		return true;
	}
}
