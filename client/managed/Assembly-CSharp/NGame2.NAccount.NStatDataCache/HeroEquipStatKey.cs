using NShared;

namespace NGame2.NAccount.NStatDataCache;

public struct HeroEquipStatKey
{
	public EquipItemKey[] ItemArray { get; private set; }

	public HeroEquipStatKey(CachedHeroInfo heroInfo)
	{
		EquipItemInfo[] equipItemInfos = heroInfo.GetEquipItemInfos();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(equipItemInfos))
		{
			ItemArray = null;
			return;
		}
		ItemArray = new EquipItemKey[__Extension_Method_ICollection__.COUNT(equipItemInfos)];
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ItemArray); i++)
		{
			ItemArray[i] = new EquipItemKey(equipItemInfos[i]);
		}
	}

	public static bool IsEquals(HeroEquipStatKey x, HeroEquipStatKey y)
	{
		if (__Extension_Method_ICollection__.COUNT(x.ItemArray) != __Extension_Method_ICollection__.COUNT(y.ItemArray))
		{
			return false;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(x.ItemArray); i++)
		{
			EquipItemKey x2 = x.ItemArray[i];
			EquipItemKey y2 = y.ItemArray[i];
			if (!EquipItemKey.IsEquals(x2, y2))
			{
				return false;
			}
		}
		return true;
	}

	public static int GetHashCode(HeroEquipStatKey obj)
	{
		int num = 31;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(obj.ItemArray); i++)
		{
			EquipItemKey obj2 = obj.ItemArray[i];
			num = num * 17 + EquipItemKey.GetHashCode(obj2);
		}
		return num;
	}
}
