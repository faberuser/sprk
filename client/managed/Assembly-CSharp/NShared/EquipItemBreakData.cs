using System.Collections.Generic;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EquipItemBreakData : IDataKeySelector<EquipItemBreakDataKey>
{
	public class BreakItemInfo
	{
		public string ItemCode { get; set; }

		public int MinItemCount { get; set; }

		public int MaxItemCount { get; set; }
	}

	EquipItemBreakDataKey IDataKeySelector<EquipItemBreakDataKey>.Key => new EquipItemBreakDataKey(Tier, DetailIndex, ItemCode, Star, SetIndex);

	[Key(0)]
	public int Tier { get; set; }

	[Key(1)]
	public int DetailIndex { get; set; }

	[Key(2)]
	public string ItemCode { get; set; }

	[Key(3)]
	public int Star { get; set; }

	[Key(4)]
	public string ItemCode1 { get; set; }

	[Key(5)]
	public int MinItemCount1 { get; set; }

	[Key(6)]
	public int MaxItemCount1 { get; set; }

	[Key(7)]
	public int ItemCount1_1 { get; set; }

	[Key(8)]
	public int ItemRate1_1 { get; set; }

	[Key(9)]
	public int ItemCount1_2 { get; set; }

	[Key(10)]
	public int ItemRate1_2 { get; set; }

	[Key(11)]
	public string ItemCode2 { get; set; }

	[Key(12)]
	public int MinItemCount2 { get; set; }

	[Key(13)]
	public int MaxItemCount2 { get; set; }

	[Key(14)]
	public int ItemCount2_1 { get; set; }

	[Key(15)]
	public int ItemRate2_1 { get; set; }

	[Key(16)]
	public int ItemCount2_2 { get; set; }

	[Key(17)]
	public int ItemRate2_2 { get; set; }

	[Key(18)]
	public string ItemCode3 { get; set; }

	[Key(19)]
	public int MinItemCount3 { get; set; }

	[Key(20)]
	public int MaxItemCount3 { get; set; }

	[Key(21)]
	public int ItemCount3_1 { get; set; }

	[Key(22)]
	public int ItemRate3_1 { get; set; }

	[Key(23)]
	public int ItemCount3_2 { get; set; }

	[Key(24)]
	public int ItemRate3_2 { get; set; }

	[Key(25)]
	public string ItemCode4 { get; set; }

	[Key(26)]
	public int MinItemCount4 { get; set; }

	[Key(27)]
	public int MaxItemCount4 { get; set; }

	[Key(28)]
	public int ItemCount4_1 { get; set; }

	[Key(29)]
	public int ItemRate4_1 { get; set; }

	[Key(30)]
	public int ItemCount4_2 { get; set; }

	[Key(31)]
	public int ItemRate4_2 { get; set; }

	[Key(32)]
	public string ItemCode5 { get; set; }

	[Key(33)]
	public int MinItemCount5 { get; set; }

	[Key(34)]
	public int MaxItemCount5 { get; set; }

	[Key(35)]
	public int ItemCount5_1 { get; set; }

	[Key(36)]
	public int ItemRate5_1 { get; set; }

	[Key(37)]
	public int ItemCount5_2 { get; set; }

	[Key(38)]
	public int ItemRate5_2 { get; set; }

	[Key(39)]
	public int SetIndex { get; set; }

	public override string ToString()
	{
		return "Tier=" + Tier + " Grade=" + DetailIndex + " ItemCode=" + ItemCode;
	}

	public List<BreakItemInfo> GetAllBreakItemInfo()
	{
		List<BreakItemInfo> list = new List<BreakItemInfo>();
		if (!string.IsNullOrEmpty(ItemCode1))
		{
			BreakItemInfo breakItemInfo = CreateBreakInfo(ItemCode1, MinItemCount1, MaxItemCount1, ItemCount1_1, ItemCount1_2);
			if (breakItemInfo != null)
			{
				list.Add(breakItemInfo);
			}
		}
		if (!string.IsNullOrEmpty(ItemCode2))
		{
			BreakItemInfo breakItemInfo2 = CreateBreakInfo(ItemCode2, MinItemCount2, MaxItemCount2, ItemCount2_1, ItemCount2_2);
			if (breakItemInfo2 != null)
			{
				list.Add(breakItemInfo2);
			}
		}
		if (!string.IsNullOrEmpty(ItemCode3))
		{
			BreakItemInfo breakItemInfo3 = CreateBreakInfo(ItemCode3, MinItemCount3, MaxItemCount3, ItemCount3_1, ItemCount3_2);
			if (breakItemInfo3 != null)
			{
				list.Add(breakItemInfo3);
			}
		}
		if (!string.IsNullOrEmpty(ItemCode4))
		{
			BreakItemInfo breakItemInfo4 = CreateBreakInfo(ItemCode4, MinItemCount4, MaxItemCount4, ItemCount4_1, ItemCount4_2);
			if (breakItemInfo4 != null)
			{
				list.Add(breakItemInfo4);
			}
		}
		if (!string.IsNullOrEmpty(ItemCode5))
		{
			BreakItemInfo breakItemInfo5 = CreateBreakInfo(ItemCode5, MinItemCount5, MaxItemCount5, ItemCount5_1, ItemCount5_2);
			if (breakItemInfo5 != null)
			{
				list.Add(breakItemInfo5);
			}
		}
		return list;
	}

	public static BreakItemInfo CreateBreakInfo(string itemCode, int min, int max, int rateMin, int rateMax)
	{
		if (rateMin != 0 || rateMax != 0)
		{
			return new BreakItemInfo
			{
				ItemCode = itemCode,
				MinItemCount = rateMin,
				MaxItemCount = rateMax
			};
		}
		if (min != 0 || max != 0)
		{
			return new BreakItemInfo
			{
				ItemCode = itemCode,
				MinItemCount = min,
				MaxItemCount = max
			};
		}
		return null;
	}
}
