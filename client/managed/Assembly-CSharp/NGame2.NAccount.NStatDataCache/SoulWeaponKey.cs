using NShared;

namespace NGame2.NAccount.NStatDataCache;

public struct SoulWeaponKey
{
	public int EquipSlotIndex { get; private set; }

	public int Grade { get; private set; }

	public int ReinforceLevel { get; private set; }

	public int OptionRatio1 { get; private set; }

	public int OptionRatio2 { get; private set; }

	public int OptionBonusRatio1 { get; private set; }

	public int OptionBonusRatio2 { get; private set; }

	public SoulWeaponKey(EquipItemInfo info)
	{
		if (!info.IsSoulWeapon())
		{
			EquipSlotIndex = 0;
			Grade = 0;
			ReinforceLevel = 0;
			OptionRatio1 = 0;
			OptionRatio2 = 0;
			OptionBonusRatio1 = 0;
			OptionBonusRatio2 = 0;
		}
		else
		{
			SoulWeaponInfo soulWeaponInfo = info.SoulWeaponInfo;
			EquipSlotIndex = soulWeaponInfo.EquipItemSlotIndex;
			ReinforceLevel = soulWeaponInfo.ReinforceLevel;
			Grade = soulWeaponInfo.Grade;
			OptionRatio1 = soulWeaponInfo.OptionRatio1;
			OptionRatio2 = soulWeaponInfo.OptionRatio2;
			OptionBonusRatio1 = soulWeaponInfo.OptionBonusRatio1;
			OptionBonusRatio2 = soulWeaponInfo.OptionBonusRatio2;
		}
	}

	public static bool IsEquals(SoulWeaponKey x, SoulWeaponKey y)
	{
		if (x.EquipSlotIndex != y.EquipSlotIndex)
		{
			return false;
		}
		if (x.Grade != y.Grade)
		{
			return false;
		}
		if (x.ReinforceLevel != y.ReinforceLevel)
		{
			return false;
		}
		if (x.OptionRatio1 != y.OptionRatio1)
		{
			return false;
		}
		if (x.OptionRatio2 != y.OptionRatio2)
		{
			return false;
		}
		if (x.OptionBonusRatio1 != y.OptionBonusRatio1)
		{
			return false;
		}
		if (x.OptionBonusRatio2 != y.OptionBonusRatio2)
		{
			return false;
		}
		return true;
	}

	public static int GetHashCode(SoulWeaponKey obj)
	{
		return ((((((31 * 17 + obj.EquipSlotIndex) * 17 + obj.Grade) * 17 + obj.ReinforceLevel) * 17 + obj.OptionRatio1) * 17 + obj.OptionRatio2) * 17 + obj.OptionBonusRatio1) * 17 + obj.OptionBonusRatio2;
	}
}
