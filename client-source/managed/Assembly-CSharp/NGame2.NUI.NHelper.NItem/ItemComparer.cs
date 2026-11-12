using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;

namespace NGame2.NUI.NHelper.NItem;

public static class ItemComparer
{
	public interface CompareClassInterace
	{
		void SetNextCompare(CompareClassInterace nextCompare);

		int Compare();
	}

	public class SortClass
	{
		private CompareClassInterace Begin;

		private CompareClassInterace Last;

		public SortClass Add<T>(Func<T, T, bool, int> func, T val1, T val2, bool isAscending) where T : IComparable
		{
			CompareClassInterace compareClassInterace = new CompareClass<T>(func, val1, val2, isAscending);
			if (Begin == null)
			{
				Begin = compareClassInterace;
			}
			else
			{
				Last.SetNextCompare(compareClassInterace);
			}
			Last = compareClassInterace;
			return this;
		}

		public int Sort()
		{
			return Begin.Compare();
		}
	}

	public class CompareClass<T> : CompareClassInterace where T : IComparable
	{
		public Func<T, T, bool, int> OnCompareClass;

		public T Value1;

		public T Value2;

		public bool IsAscending;

		private CompareClassInterace NextCompare;

		public CompareClass(Func<T, T, bool, int> func, T val1, T val2, bool isAscending)
		{
			OnCompareClass = func;
			Value1 = val1;
			Value2 = val2;
			IsAscending = isAscending;
		}

		int CompareClassInterace.Compare()
		{
			int num = OnCompareClass(Value1, Value2, IsAscending);
			if (num == 0 && NextCompare != null)
			{
				num = NextCompare.Compare();
			}
			return num;
		}

		void CompareClassInterace.SetNextCompare(CompareClassInterace nextCompare)
		{
			NextCompare = nextCompare;
		}
	}

	private interface IComparerWrapper
	{
	}

	private class ComparerWrapper<T> : IComparerWrapper
	{
		public Func<T, T, int> CompareFunc;
	}

	private static IncreaseDic<EquipPartType, int> EquipPartOrderDic;

	private static Dictionary<Type, IComparerWrapper> ComparerWrapperDic;

	static ItemComparer()
	{
		ComparerWrapperDic = new Dictionary<Type, IComparerWrapper>();
		EquipPartOrderDic = new IncreaseDic<EquipPartType, int>();
		EquipPartOrderDic.Add(EquipPartType.Weapon, EquipPartOrderDic.Count);
		EquipPartOrderDic.Add(EquipPartType.Armor, EquipPartOrderDic.Count);
		EquipPartOrderDic.Add(EquipPartType.SecondGear, EquipPartOrderDic.Count);
		EquipPartOrderDic.Add(EquipPartType.Accessory, EquipPartOrderDic.Count);
		EquipPartOrderDic.Add(EquipPartType.Orb, EquipPartOrderDic.Count);
		EquipPartOrderDic.Add(EquipPartType.Artifact, EquipPartOrderDic.Count);
		EquipPartOrderDic.Add(EquipPartType.Treasure, EquipPartOrderDic.Count);
		EquipPartOrderDic.Add(EquipPartType.EnhanceStone, EquipPartOrderDic.Count);
		ComparerWrapperDic.Add(typeof(int), new ComparerWrapper<int>
		{
			CompareFunc = (int a, int b) => a.CompareTo(b)
		});
		ComparerWrapperDic.Add(typeof(short), new ComparerWrapper<short>
		{
			CompareFunc = (short a, short b) => a.CompareTo(b)
		});
		ComparerWrapperDic.Add(typeof(byte), new ComparerWrapper<byte>
		{
			CompareFunc = (byte a, byte b) => a.CompareTo(b)
		});
		ComparerWrapperDic.Add(typeof(bool), new ComparerWrapper<bool>
		{
			CompareFunc = (bool a, bool b) => a.CompareTo(b)
		});
		ComparerWrapperDic.Add(typeof(DateTime), new ComparerWrapper<DateTime>
		{
			CompareFunc = (DateTime a, DateTime b) => a.CompareTo(b)
		});
		ComparerWrapperDic.Add(typeof(EquipItemSubType), new ComparerWrapper<EquipItemSubType>
		{
			CompareFunc = delegate(EquipItemSubType a, EquipItemSubType b)
			{
				int num = (int)a;
				return num.CompareTo((int)b);
			}
		});
	}

	public static int Compare(EquipItemInfo lhs, EquipItemInfo rhs)
	{
		int num = 0;
		EquipItemData equipItemData = lhs.EquipItemData;
		EquipItemData equipItemData2 = rhs.EquipItemData;
		ItemData itemData = lhs.ItemData;
		ItemData itemData2 = rhs.ItemData;
		if (num == 0)
		{
			num = Compare(equipItemData.Tier, equipItemData2.Tier, isAscending: false);
		}
		if (num == 0)
		{
			int grade = ((equipItemData.PartType == EquipPartType.EnhanceStone) ? 1 : itemData.Grade);
			byte grade2 = (byte)((equipItemData2.PartType == EquipPartType.EnhanceStone) ? 1 : itemData2.Grade);
			num = Compare_ItemGrade((byte)grade, grade2, isAscending: false);
		}
		if (num == 0)
		{
			num = Compare_WeaponType(equipItemData.PartType, equipItemData2.PartType, isAscending: true);
		}
		if (num == 0)
		{
			num = Compare_SetGroupType(equipItemData.SetGroupType, equipItemData2.SetGroupType, isAscending: true);
		}
		if (num == 0)
		{
			num = Compare_Identified(lhs.IsIdentified, rhs.IsIdentified, isAscending: true);
		}
		if (num == 0)
		{
			num = Compare(equipItemData.SubType, equipItemData2.SubType, isAscending: true);
		}
		if (num == 0)
		{
			num = Compare(itemData.Index, itemData2.Index, isAscending: true);
		}
		if (num == 0)
		{
			num = Compare(lhs.Locked, rhs.Locked, isAscending: false);
		}
		if (num == 0)
		{
			num = Compare(lhs.Star, rhs.Star, isAscending: false);
		}
		if (num == 0)
		{
			num = Compare(lhs.Level, rhs.Level, isAscending: false);
		}
		if (num == 0)
		{
			num = Compare_CreateTime(lhs.CreateDateTime, rhs.CreateDateTime, isAscending: true);
		}
		return num;
	}

	public static int Sort(ItemIconComponent comp1, ItemIconComponent comp2)
	{
		int num = 0;
		num = NullCheck(comp1, comp2);
		if (num != 0)
		{
			return num;
		}
		ItemData itemData = comp1.ItemData;
		ItemData itemData2 = comp2.ItemData;
		if (itemData.Type == ItemType.Equip)
		{
			num = Compare(comp1.EquipItemInfo, comp2.EquipItemInfo);
		}
		else
		{
			int num2 = ((itemData.Type == ItemType.AwakeMaterial) ? itemData.GetValueInt(0) : 0);
			int value = ((itemData2.Type == ItemType.AwakeMaterial) ? itemData2.GetValueInt(0) : 0);
			num = -num2.CompareTo(value);
			if (num == 0)
			{
				num = Compare_ItemGrade(itemData.Grade, itemData2.Grade, isAscending: false);
			}
			if (num == 0)
			{
				num = itemData.Index.CompareTo(itemData2.Index);
			}
		}
		return num;
	}

	public static int ItemListBookSort(ItemData itemData1, ItemData itemData2)
	{
		int num = 0;
		if (itemData1.Type == ItemType.Equip)
		{
			EquipItemData subData = itemData1.GetSubData<EquipItemData>();
			EquipItemData subData2 = itemData2.GetSubData<EquipItemData>();
			if (num == 0)
			{
				int grade = ((subData.PartType == EquipPartType.EnhanceStone) ? 1 : itemData1.Grade);
				byte grade2 = (byte)((subData2.PartType == EquipPartType.EnhanceStone) ? 1 : itemData2.Grade);
				num = Compare_ItemGrade((byte)grade, grade2, isAscending: false);
			}
			if (num == 0)
			{
				num = Compare(subData.Tier, subData2.Tier, isAscending: false);
			}
			if (num == 0)
			{
				num = Compare_WeaponType(subData.PartType, subData2.PartType, isAscending: true);
			}
			if (num == 0)
			{
				num = Compare_SetGroupType(subData.SetGroupType, subData2.SetGroupType, isAscending: true);
			}
			if (num == 0)
			{
				num = Compare(subData.SubType, subData2.SubType, isAscending: true);
			}
			if (num == 0)
			{
				num = Compare(itemData1.Index, itemData2.Index, isAscending: true);
			}
		}
		else
		{
			if (num == 0)
			{
				num = Compare_ItemGrade(itemData1.Grade, itemData2.Grade, isAscending: false);
			}
			if (num == 0)
			{
				num = itemData1.Index.CompareTo(itemData2.Index);
			}
		}
		return num;
	}

	public static int NullCheck(ItemIconComponent comp1, ItemIconComponent comp2)
	{
		if (comp1 == comp2)
		{
			return 0;
		}
		int num = 0;
		num = CompareRefrence(comp1, comp2, null);
		if (num != 0)
		{
			return num;
		}
		ItemData itemData = comp1.ItemData;
		ItemData itemData2 = comp2.ItemData;
		num = CompareRefrence(itemData, itemData2, null);
		if (num != 0 || (itemData == null && itemData2 == null))
		{
			return num;
		}
		if (itemData.Type == ItemType.Equip)
		{
			EquipItemInfo equipItemInfo = comp1.EquipItemInfo;
			EquipItemInfo equipItemInfo2 = comp2.EquipItemInfo;
			num = CompareRefrence(equipItemInfo, equipItemInfo2, null);
			if (num != 0 || (equipItemInfo == null && equipItemInfo2 == null))
			{
				return num;
			}
			EquipItemData equipItemData = comp1.EquipItemData;
			EquipItemData equipItemData2 = comp2.EquipItemData;
			num = CompareRefrence(equipItemData, equipItemData2, null);
			if (num != 0 || (equipItemData == null && equipItemData2 == null))
			{
				return num;
			}
		}
		return 0;
	}

	public static int CompareValue<T>(T a, T b, T value) where T : struct
	{
		if (a.Equals(value) && !b.Equals(value))
		{
			return -1;
		}
		if (b.Equals(value) && !a.Equals(value))
		{
			return 1;
		}
		return 0;
	}

	public static int CompareRefrence<T>(T a, T b, T value) where T : class
	{
		if (a == value && b != value)
		{
			return -1;
		}
		if (b == value && a != value)
		{
			return 1;
		}
		return 0;
	}

	public static int Compare<T>(T a, T b, bool isAscending) where T : IComparable
	{
		int num = ((Vespa_Generic_IDictionary_Extension_Method.GETVALUE(ComparerWrapperDic, typeof(T), null) is ComparerWrapper<T> comparerWrapper) ? comparerWrapper.CompareFunc(a, b) : a.CompareTo(b));
		if (!isAscending)
		{
			num = -num;
		}
		return num;
	}

	public static int Compare_ItemGrade(byte grade1, byte grade2, bool isAscending)
	{
		ItemGradeData data = GameRoot.Instance.ItemGradeDataContainer.GetData(grade1);
		ItemGradeData data2 = GameRoot.Instance.ItemGradeDataContainer.GetData(grade2);
		byte a = data?.Score ?? 1;
		byte b = data2?.Score ?? 1;
		return Compare(a, b, isAscending);
	}

	public static int Compare_WeaponType(EquipPartType type1, EquipPartType type2, bool isAscending)
	{
		int vALUE = EquipPartOrderDic.GETVALUE(type1, 0);
		int vALUE2 = EquipPartOrderDic.GETVALUE(type2, 0);
		return Compare(vALUE, vALUE2, isAscending);
	}

	public static int Compare_CreateTime(DateTime time1, DateTime time2, bool isAscending)
	{
		return Compare(time1, time2, isAscending);
	}

	public static int Compare_SetGroupType(EquipItemSetGroupType type1, EquipItemSetGroupType type2, bool isAscending)
	{
		int num = type1.CompareTo(type2);
		if (!isAscending)
		{
			return -num;
		}
		return num;
	}

	public static int Compare_Identified(bool identified1, bool identified2, bool isAscending)
	{
		int num = identified1.CompareTo(identified2);
		if (!isAscending)
		{
			return -num;
		}
		return num;
	}

	public static int Compare_TechnoMagic_Advance(bool advance1, bool advance2, bool isAscending)
	{
		int num = advance1.CompareTo(advance2);
		if (!isAscending)
		{
			return -num;
		}
		return num;
	}

	public static int Compare_EquipItemInfo(EquipItemInfo lhsEquipItemInfo, EquipItemInfo rhsEquipItemInfo)
	{
		if (lhsEquipItemInfo == rhsEquipItemInfo)
		{
			return 0;
		}
		int num = CompareRefrence(lhsEquipItemInfo, rhsEquipItemInfo, null);
		if (num != 0)
		{
			return num;
		}
		ItemData itemData = lhsEquipItemInfo.ItemData;
		ItemData itemData2 = rhsEquipItemInfo.ItemData;
		num = CompareRefrence(itemData, itemData2, null);
		if (num != 0 && itemData == null && itemData2 == null)
		{
			return num;
		}
		EquipItemData equipItemData = lhsEquipItemInfo.EquipItemData;
		EquipItemData equipItemData2 = rhsEquipItemInfo.EquipItemData;
		num = CompareRefrence(equipItemData, equipItemData2, null);
		if (num != 0 && equipItemData == null && equipItemData2 == null)
		{
			return num;
		}
		if (itemData.Tutorial)
		{
			return -1;
		}
		if (itemData2.Tutorial)
		{
			return 1;
		}
		num = Compare(lhsEquipItemInfo.IsEquipped, rhsEquipItemInfo.IsEquipped, isAscending: false);
		if (num != 0)
		{
			return num;
		}
		num = Compare(equipItemData.Tier, equipItemData.Tier, isAscending: false);
		if (num != 0)
		{
			return num;
		}
		num = Compare_ItemGrade(itemData.Grade, itemData2.Grade, isAscending: false);
		if (num != 0)
		{
			return num;
		}
		num = Compare_Identified(lhsEquipItemInfo.IsIdentified, rhsEquipItemInfo.IsIdentified, isAscending: true);
		if (num != 0)
		{
			return num;
		}
		num = Compare_TechnoMagic_Advance(lhsEquipItemInfo.IsAdvanced, rhsEquipItemInfo.IsAdvanced, isAscending: false);
		if (num != 0)
		{
			return num;
		}
		num = Compare_WeaponType(equipItemData.PartType, equipItemData2.PartType, isAscending: true);
		if (num != 0)
		{
			return num;
		}
		num = Compare(equipItemData.SubType, equipItemData2.SubType, isAscending: true);
		if (num != 0)
		{
			return num;
		}
		num = Compare(itemData.Index, itemData2.Index, isAscending: true);
		if (num != 0)
		{
			return num;
		}
		num = Compare(lhsEquipItemInfo.Locked, rhsEquipItemInfo.Locked, isAscending: false);
		if (num != 0)
		{
			return num;
		}
		num = Compare(lhsEquipItemInfo.Star, rhsEquipItemInfo.Star, isAscending: false);
		if (num != 0)
		{
			return num;
		}
		num = Compare(lhsEquipItemInfo.Level, rhsEquipItemInfo.Level, isAscending: false);
		if (num != 0)
		{
			return num;
		}
		return Compare_CreateTime(lhsEquipItemInfo.CreateDateTime, rhsEquipItemInfo.CreateDateTime, isAscending: true);
	}
}
