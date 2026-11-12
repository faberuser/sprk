using System.Collections.Generic;
using NShared;

namespace NGame2.NAccount.NStatDataCache;

public struct CachedHeroKey
{
	public StatCalculator.HeroStatCalculationFlag CalculationFlag { get; private set; }

	public HeroKey Hero { get; private set; }

	public EquipItemKey Equip1 { get; private set; }

	public EquipItemKey Equip2 { get; private set; }

	public EquipItemKey Equip3 { get; private set; }

	public EquipItemKey Equip4 { get; private set; }

	public EquipItemKey Equip5 { get; private set; }

	public EquipItemKey Equip6 { get; private set; }

	public EquipItemKey Equip7 { get; private set; }

	public EquipItemKey Equip8 { get; private set; }

	public EquipItemKey Equip9 { get; private set; }

	public EquipItemKey Equip10 { get; private set; }

	public ItemOptionKey Option1 { get; private set; }

	public ItemOptionKey Option2 { get; private set; }

	public ItemOptionKey Option3 { get; private set; }

	public ItemOptionKey Option4 { get; private set; }

	public ItemOptionKey Option5 { get; private set; }

	public ItemOptionKey Option6 { get; private set; }

	public ItemOptionKey Option7 { get; private set; }

	public ItemOptionKey Option8 { get; private set; }

	public ItemOptionKey Option9 { get; private set; }

	public ItemOptionKey Option10 { get; private set; }

	public SoulWeaponKey SoulWeapon { get; private set; }

	public RuneStatKey[] RunePage { get; private set; }

	public CachedHeroKey(CachedHeroInfo info, StatCalculator.HeroStatCalculationFlag calculationFlag)
	{
		CalculationFlag = calculationFlag;
		bool flag = (calculationFlag & StatCalculator.HeroStatCalculationFlag.Equip) == StatCalculator.HeroStatCalculationFlag.Equip;
		bool flag2 = (calculationFlag & StatCalculator.HeroStatCalculationFlag.EquipOption) == StatCalculator.HeroStatCalculationFlag.EquipOption;
		Hero = new HeroKey(info);
		Equip1 = ((flag && info.EquipItemInfo1 != null) ? new EquipItemKey(info.EquipItemInfo1) : default(EquipItemKey));
		Equip2 = ((flag && info.EquipItemInfo2 != null) ? new EquipItemKey(info.EquipItemInfo2) : default(EquipItemKey));
		Equip3 = ((flag && info.EquipItemInfo3 != null) ? new EquipItemKey(info.EquipItemInfo3) : default(EquipItemKey));
		Equip4 = ((flag && info.EquipItemInfo4 != null) ? new EquipItemKey(info.EquipItemInfo4) : default(EquipItemKey));
		Equip5 = ((flag && info.EquipItemInfo5 != null) ? new EquipItemKey(info.EquipItemInfo5) : default(EquipItemKey));
		Equip6 = ((flag && info.EquipItemInfo6 != null) ? new EquipItemKey(info.EquipItemInfo6) : default(EquipItemKey));
		Equip7 = ((flag && info.EquipItemInfo7 != null) ? new EquipItemKey(info.EquipItemInfo7) : default(EquipItemKey));
		Equip8 = ((flag && info.EquipItemInfo8 != null) ? new EquipItemKey(info.EquipItemInfo8) : default(EquipItemKey));
		Equip9 = ((flag && info.EquipItemInfo9 != null) ? new EquipItemKey(info.EquipItemInfo9) : default(EquipItemKey));
		Equip10 = ((flag && info.EquipItemInfo10 != null) ? new EquipItemKey(info.EquipItemInfo10) : default(EquipItemKey));
		Option1 = ((flag2 && info.EquipItemInfo1 != null) ? new ItemOptionKey(info.EquipItemInfo1) : default(ItemOptionKey));
		Option2 = ((flag2 && info.EquipItemInfo2 != null) ? new ItemOptionKey(info.EquipItemInfo2) : default(ItemOptionKey));
		Option3 = ((flag2 && info.EquipItemInfo3 != null) ? new ItemOptionKey(info.EquipItemInfo3) : default(ItemOptionKey));
		Option4 = ((flag2 && info.EquipItemInfo4 != null) ? new ItemOptionKey(info.EquipItemInfo4) : default(ItemOptionKey));
		Option5 = ((flag2 && info.EquipItemInfo5 != null) ? new ItemOptionKey(info.EquipItemInfo5) : default(ItemOptionKey));
		Option6 = ((flag2 && info.EquipItemInfo6 != null) ? new ItemOptionKey(info.EquipItemInfo6) : default(ItemOptionKey));
		Option7 = ((flag2 && info.EquipItemInfo7 != null) ? new ItemOptionKey(info.EquipItemInfo7) : default(ItemOptionKey));
		Option8 = ((flag2 && info.EquipItemInfo8 != null) ? new ItemOptionKey(info.EquipItemInfo8) : default(ItemOptionKey));
		Option9 = ((flag2 && info.EquipItemInfo9 != null) ? new ItemOptionKey(info.EquipItemInfo9) : default(ItemOptionKey));
		Option10 = ((flag2 && info.EquipItemInfo10 != null) ? new ItemOptionKey(info.EquipItemInfo10) : default(ItemOptionKey));
		SoulWeapon = ((flag && info.EquipItemInfo1 != null) ? new SoulWeaponKey(info.EquipItemInfo1) : default(SoulWeaponKey));
		List<HeroRunePageInfo> runePageInfos = info.GetRunePageInfos(info.ApplyRunePage);
		if (runePageInfos != null && flag2)
		{
			RunePage = new RuneStatKey[runePageInfos.Count];
			for (int i = 0; i < runePageInfos.Count; i++)
			{
				RunePage[i] = new RuneStatKey(runePageInfos[i]);
			}
		}
		else
		{
			RunePage = new RuneStatKey[1];
			RunePage[0] = default(RuneStatKey);
		}
	}

	public static bool IsEquals(CachedHeroKey x, CachedHeroKey y)
	{
		if (x.CalculationFlag != y.CalculationFlag)
		{
			return false;
		}
		if (!HeroKey.IsEquals(x.Hero, y.Hero))
		{
			return false;
		}
		if (!EquipItemKey.IsEquals(x.Equip1, y.Equip1))
		{
			return false;
		}
		if (!EquipItemKey.IsEquals(x.Equip2, y.Equip2))
		{
			return false;
		}
		if (!EquipItemKey.IsEquals(x.Equip3, y.Equip3))
		{
			return false;
		}
		if (!EquipItemKey.IsEquals(x.Equip4, y.Equip4))
		{
			return false;
		}
		if (!EquipItemKey.IsEquals(x.Equip5, y.Equip5))
		{
			return false;
		}
		if (!EquipItemKey.IsEquals(x.Equip6, y.Equip6))
		{
			return false;
		}
		if (!EquipItemKey.IsEquals(x.Equip7, y.Equip7))
		{
			return false;
		}
		if (!EquipItemKey.IsEquals(x.Equip8, y.Equip8))
		{
			return false;
		}
		if (!EquipItemKey.IsEquals(x.Equip9, y.Equip9))
		{
			return false;
		}
		if (!EquipItemKey.IsEquals(x.Equip10, y.Equip10))
		{
			return false;
		}
		if (!ItemOptionKey.IsEquals(x.Option1, y.Option1))
		{
			return false;
		}
		if (!ItemOptionKey.IsEquals(x.Option2, y.Option2))
		{
			return false;
		}
		if (!ItemOptionKey.IsEquals(x.Option3, y.Option3))
		{
			return false;
		}
		if (!ItemOptionKey.IsEquals(x.Option4, y.Option4))
		{
			return false;
		}
		if (!ItemOptionKey.IsEquals(x.Option5, y.Option5))
		{
			return false;
		}
		if (!ItemOptionKey.IsEquals(x.Option6, y.Option6))
		{
			return false;
		}
		if (!ItemOptionKey.IsEquals(x.Option7, y.Option7))
		{
			return false;
		}
		if (!ItemOptionKey.IsEquals(x.Option8, y.Option8))
		{
			return false;
		}
		if (!ItemOptionKey.IsEquals(x.Option9, y.Option9))
		{
			return false;
		}
		if (!ItemOptionKey.IsEquals(x.Option10, y.Option10))
		{
			return false;
		}
		if (!SoulWeaponKey.IsEquals(x.SoulWeapon, y.SoulWeapon))
		{
			return false;
		}
		if (x.RunePage.Length != x.RunePage.Length)
		{
			return false;
		}
		for (int i = 0; i < x.RunePage.Length; i++)
		{
			if (!RuneStatKey.IsEquals(x.RunePage[i], y.RunePage[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static int GetHashCode(CachedHeroKey obj)
	{
		int calculationFlag = (int)obj.CalculationFlag;
		calculationFlag = calculationFlag * 17 + HeroKey.GetHashCode(obj.Hero);
		calculationFlag = calculationFlag * 17 + EquipItemKey.GetHashCode(obj.Equip1);
		calculationFlag = calculationFlag * 17 + EquipItemKey.GetHashCode(obj.Equip2);
		calculationFlag = calculationFlag * 17 + EquipItemKey.GetHashCode(obj.Equip3);
		calculationFlag = calculationFlag * 17 + EquipItemKey.GetHashCode(obj.Equip4);
		calculationFlag = calculationFlag * 17 + EquipItemKey.GetHashCode(obj.Equip5);
		calculationFlag = calculationFlag * 17 + EquipItemKey.GetHashCode(obj.Equip6);
		calculationFlag = calculationFlag * 17 + EquipItemKey.GetHashCode(obj.Equip7);
		calculationFlag = calculationFlag * 17 + EquipItemKey.GetHashCode(obj.Equip8);
		calculationFlag = calculationFlag * 17 + EquipItemKey.GetHashCode(obj.Equip9);
		calculationFlag = calculationFlag * 17 + EquipItemKey.GetHashCode(obj.Equip10);
		calculationFlag = calculationFlag * 17 + ItemOptionKey.GetHashCode(obj.Option1);
		calculationFlag = calculationFlag * 17 + ItemOptionKey.GetHashCode(obj.Option2);
		calculationFlag = calculationFlag * 17 + ItemOptionKey.GetHashCode(obj.Option3);
		calculationFlag = calculationFlag * 17 + ItemOptionKey.GetHashCode(obj.Option4);
		calculationFlag = calculationFlag * 17 + ItemOptionKey.GetHashCode(obj.Option5);
		calculationFlag = calculationFlag * 17 + ItemOptionKey.GetHashCode(obj.Option6);
		calculationFlag = calculationFlag * 17 + ItemOptionKey.GetHashCode(obj.Option7);
		calculationFlag = calculationFlag * 17 + ItemOptionKey.GetHashCode(obj.Option8);
		calculationFlag = calculationFlag * 17 + ItemOptionKey.GetHashCode(obj.Option9);
		calculationFlag = calculationFlag * 17 + ItemOptionKey.GetHashCode(obj.Option10);
		calculationFlag = calculationFlag * 17 + SoulWeaponKey.GetHashCode(obj.SoulWeapon);
		RuneStatKey[] runePage = obj.RunePage;
		foreach (RuneStatKey obj2 in runePage)
		{
			calculationFlag = calculationFlag * 17 + RuneStatKey.GetHashCode(obj2);
		}
		return calculationFlag;
	}
}
