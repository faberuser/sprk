using System.Collections.Generic;
using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
public sealed class RewardData : IDataKeySelector<int>
{
	[MessagePackObject(false)]
	public class SubData
	{
		[Key(0)]
		public int No { get; set; }

		[Key(1)]
		public HashString Code { get; set; }

		[Key(2)]
		public int Rate { get; set; }

		[Key(3)]
		public int CountMin { get; set; }

		[Key(5)]
		public int CountMax { get; set; }

		[Key(6)]
		public int StarMin { get; set; }

		[Key(7)]
		public int StarMax { get; set; }

		[Key(8)]
		public int CustomOptionIndex { get; set; }

		public RewardElementData CreateReward()
		{
			return new RewardElementData(Rate, Code, CountMin, CountMax, StarMin, StarMax, CustomOptionIndex);
		}
	}

	private Dictionary<int, SubData> _subDataCache;

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int GoldRate { get; set; }

	[Key(2)]
	public int GoldMin { get; set; }

	[Key(3)]
	public int GoldMax { get; set; }

	[Key(4)]
	public int GemRate { get; set; }

	[Key(5)]
	public int GemMin { get; set; }

	[Key(6)]
	public int GemMax { get; set; }

	[Key(7)]
	public int LuaPointRate { get; set; }

	[Key(8)]
	public int LuaPointMin { get; set; }

	[Key(9)]
	public int LuaPointMax { get; set; }

	[Key(10)]
	public int ShopEventPointRate { get; set; }

	[Key(11)]
	public int ShopEventPointMin { get; set; }

	[Key(12)]
	public int ShopEventPointMax { get; set; }

	[Key(13)]
	public int LimitedShopEventPointRate { get; set; }

	[Key(14)]
	public int LimitedShopEventPointValue { get; set; }

	[Key(15)]
	public List<SubData> SubDataList { get; set; }

	[IgnoreMember]
	public HashString ItemCode1 => GetItemCode(1);

	[IgnoreMember]
	public HashString Item1Code => GetItemCode(1);

	[IgnoreMember]
	public HashString Item2Code => GetItemCode(2);

	[IgnoreMember]
	public int Item1CountMin => GetItemCountMin(1);

	[IgnoreMember]
	public int Item1CountMax => GetItemCountMax(1);

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "Index=" + Index;
	}

	private SubData GetSubDataByNo(int no)
	{
		if (_subDataCache == null)
		{
			InitSubDataCache();
		}
		if (_subDataCache.TryGetValue(no, out var value))
		{
			return value;
		}
		return null;
	}

	private void InitSubDataCache()
	{
		lock (this)
		{
			if (_subDataCache != null)
			{
				return;
			}
			_subDataCache = new Dictionary<int, SubData>();
			if (SubDataList == null)
			{
				return;
			}
			foreach (SubData subData in SubDataList)
			{
				if (_subDataCache.ContainsKey(subData.No))
				{
					JitOption.JitLogWarning($"Duplicate SubDataNo: No={subData.No} in RewardData(Key={Index})");
				}
				else
				{
					_subDataCache.Add(subData.No, subData);
				}
			}
		}
	}

	public HashString GetItemCode(int no)
	{
		return GetSubDataByNo(no)?.Code;
	}

	public int GetItemRate(int no)
	{
		return GetSubDataByNo(no)?.Rate ?? 0;
	}

	public int GetItemCountMin(int no)
	{
		return GetSubDataByNo(no)?.CountMin ?? 0;
	}

	public int GetItemCountMax(int no)
	{
		return GetSubDataByNo(no)?.CountMax ?? 0;
	}

	public RewardElementData GetElementByNo(int no)
	{
		SubData subDataByNo = GetSubDataByNo(no);
		if (subDataByNo == null)
		{
			return new RewardElementData();
		}
		return subDataByNo.CreateReward();
	}

	public List<RewardElementData> GetElementList()
	{
		List<RewardElementData> list = new List<RewardElementData>();
		if (SubDataList != null)
		{
			for (int i = 0; i < SubDataList.Count; i++)
			{
				SubData subData = SubDataList[i];
				if (subData.Rate > 0)
				{
					list.Add(subData.CreateReward());
				}
			}
		}
		return list;
	}

	private List<RewardElementData> GetRewardElementList()
	{
		List<RewardElementData> list = new List<RewardElementData>();
		if (GoldRate > 0)
		{
			list.Add(new RewardElementData(GoldRate, RewardType.Gold.ToString(), GoldMin, GoldMax, 0, 0, 0, 0, 1));
		}
		if (GemRate > 0)
		{
			list.Add(new RewardElementData(GemRate, RewardType.Gem.ToString(), GemMin, GemMax, 0, 0, 0, 0, 1));
		}
		if (LuaPointRate > 0)
		{
			list.Add(new RewardElementData(LuaPointRate, RewardType.LuaPoint.ToString(), LuaPointMin, LuaPointMax, 0, 0, 0, 0, 1));
		}
		if (ShopEventPointRate > 0)
		{
			list.Add(new RewardElementData(ShopEventPointRate, RewardType.ShopEventPoint.ToString(), ShopEventPointMin, ShopEventPointMax, 0, 0, 0, 0, 1));
		}
		if (LimitedShopEventPointRate > 0)
		{
			list.Add(new RewardElementData(LimitedShopEventPointRate, RewardType.LimitedShopEventPoint.ToString(), LimitedShopEventPointValue, LimitedShopEventPointValue, 0, 0, 0, 0, 1));
		}
		if (SubDataList != null)
		{
			for (int i = 0; i < SubDataList.Count; i++)
			{
				SubData subData = SubDataList[i];
				if (subData.Rate > 0)
				{
					list.Add(subData.CreateReward());
				}
			}
		}
		return list;
	}

	private List<RewardElementData> GetNodeIconRewardElementList()
	{
		List<RewardElementData> list = new List<RewardElementData>();
		List<RewardElementData> list2 = new List<RewardElementData>();
		List<RewardElementData> list3 = new List<RewardElementData>();
		if (SubDataList != null)
		{
			for (int i = 0; i < SubDataList.Count; i++)
			{
				SubData subData = SubDataList[i];
				if (subData.Rate > 0)
				{
					list3.Add(subData.CreateReward());
				}
			}
		}
		foreach (RewardElementData item in list3)
		{
			if (item.GetCurrencyType() == CurrencyType.None)
			{
				list.Add(item);
			}
			else
			{
				list2.Add(item);
			}
		}
		if (LuaPointRate > 0)
		{
			list2.Add(new RewardElementData(LuaPointRate, RewardType.LuaPoint.ToString(), LuaPointMin, LuaPointMax, 0, 0, 0, 0, 1));
		}
		if (ShopEventPointRate > 0)
		{
			list2.Add(new RewardElementData(ShopEventPointRate, RewardType.ShopEventPoint.ToString(), ShopEventPointMin, ShopEventPointMax, 0, 0, 0, 0, 1));
		}
		if (GemRate > 0)
		{
			list2.Add(new RewardElementData(GemRate, RewardType.Gem.ToString(), GemMin, GemMax, 0, 0, 0, 0, 1));
		}
		if (GoldRate > 0)
		{
			list2.Add(new RewardElementData(GoldRate, RewardType.Gold.ToString(), GoldMin, GoldMax, 0, 0, 0, 0, 1));
		}
		list.AddRange(list2);
		return list;
	}

	public List<RewardElementData> GetNodeIconRewardItemList()
	{
		return ItemGroupData.ExtractItemGroup(GetNodeIconRewardElementList());
	}

	public List<RewardElementData> GetRewardItemList()
	{
		return ItemGroupData.ExtractItemGroup(GetRewardElementList());
	}

	public List<RewardElementData> GetRewardOnlyItemList()
	{
		return ItemGroupData.ExtractItemOnly(GetRewardElementList());
	}

	public List<RewardElementData> GetItemGroupElementList()
	{
		List<RewardElementData> list = new List<RewardElementData>();
		List<RewardElementData> rewardElementList = GetRewardElementList();
		HashSet<HashString> hashSet = new HashSet<HashString>();
		for (int i = 0; i < rewardElementList.Count; i++)
		{
			if (GameRoot.Instance.ItemGroupDataContainer.GetData(rewardElementList[i].ItemCode) != null && !hashSet.Contains(rewardElementList[i].ItemCode))
			{
				hashSet.Add(rewardElementList[i].ItemCode);
				list.Add(rewardElementList[i]);
			}
		}
		return list;
	}

	public List<RewardElementData> GetRewardItemListWithGroupCount(out int GroupCount, bool ignoreRate = false)
	{
		List<RewardElementData> rewardElementList = GetRewardElementList();
		GroupCount = 0;
		for (int i = 0; i < rewardElementList.Count; i++)
		{
			if (GameRoot.Instance.ItemGroupDataContainer.GetData(rewardElementList[i].ItemCode) != null && rewardElementList[i].ItemRate == 1000)
			{
				GroupCount++;
			}
		}
		List<RewardElementData> list = ItemGroupData.ExtractItemGroup(rewardElementList, ignoreRate);
		List<RewardElementData> list2 = new List<RewardElementData>();
		for (int j = 0; j < list.Count; j++)
		{
			RewardElementData rewardElement = list[j];
			RewardElementData rewardElementData = list2.Find(delegate(RewardElementData element)
			{
				ItemData dataByCode2 = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardElement.ItemCode);
				return (dataByCode2 != null && dataByCode2.Type == ItemType.Equip) ? (element.ItemCode == rewardElement.ItemCode && element.ItemStarMax == rewardElement.ItemStarMax) : (element.ItemCode == rewardElement.ItemCode);
			});
			if (rewardElementData == null)
			{
				list2.Add(rewardElement);
				continue;
			}
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardElement.ItemCode);
			if (dataByCode != null && dataByCode.Type != ItemType.Equip)
			{
				rewardElementData.ItemMin += rewardElement.ItemMin;
				rewardElementData.ItemMax += rewardElement.ItemMax;
			}
		}
		return list2;
	}

	public List<string> GetRewardItemCodeList()
	{
		return ItemGroupData.GetRewardItemCodeList(GetRewardItemList());
	}

	public int GetItemMaxCount(string ItemCode)
	{
		for (int i = 0; i < SubDataList.Count; i++)
		{
			SubData subData = SubDataList[i];
			if (!string.IsNullOrEmpty(subData.Code) && !(subData.Code != (HashString)ItemCode))
			{
				return subData.CountMax;
			}
		}
		return 0;
	}

	public CurrencyType GetCurrencyType()
	{
		if (GemRate > 0)
		{
			return CurrencyType.Gem;
		}
		if (GoldRate > 0)
		{
			return CurrencyType.Gold;
		}
		return CurrencyType.None;
	}
}
