using System.Collections.Generic;
using System.Linq;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public sealed class ItemGroupData : IDataKeySelector<string>
{
	[MessagePackObject(false)]
	public class SubData
	{
		[Key(0)]
		public string ItemCode { get; set; }

		[Key(1)]
		public int ItemRate { get; set; }

		[Key(2)]
		public int ItemMin { get; set; }

		[Key(3)]
		public int ItemMax { get; set; }

		[Key(4)]
		public int ItemStar { get; set; }

		[Key(5)]
		public int ItemStarMin { get; set; }

		[Key(6)]
		public int ItemStarMax { get; set; }

		[Key(7)]
		public int Free { get; set; }
	}

	[Key(0)]
	public string ItemGroupCode { get; set; }

	[Key(1)]
	public string ItemGroupDesc { get; set; }

	[Key(2)]
	public bool ShowDetail { get; set; }

	[Key(3)]
	public string ItemGroupNameKey { get; set; }

	[Key(4)]
	public int TotalRate { get; set; }

	[Key(5)]
	public List<SubData> SubDataList { get; set; }

	string IDataKeySelector<string>.Key => ItemGroupCode;

	public override string ToString()
	{
		return "Code=" + ItemGroupCode;
	}

	public int GetTotalRate()
	{
		return TotalRate;
	}

	public List<RewardElementData> GetElementList()
	{
		List<RewardElementData> list = new List<RewardElementData>();
		if (SubDataList != null)
		{
			for (int i = 0; i < SubDataList.Count; i++)
			{
				SubData subData = SubDataList[i];
				RewardElementData rewardElementData = new RewardElementData
				{
					ItemCode = subData.ItemCode,
					ItemRate = subData.ItemRate,
					ItemMin = subData.ItemMin,
					ItemMax = subData.ItemMax,
					ItemStar = subData.ItemStar,
					ItemStarMin = subData.ItemStarMin,
					ItemStarMax = subData.ItemStarMax,
					Free = subData.Free
				};
				if (((DateElementField)rewardElementData).IsValid())
				{
					list.Add(rewardElementData);
				}
			}
		}
		return list;
	}

	public List<RewardElementData> GetRewardItemList()
	{
		return ExtractItemGroup(GetElementList());
	}

	public List<string> GetRewardItemCodeList()
	{
		return GetRewardItemCodeList(GetRewardItemList());
	}

	public static List<string> GetRewardItemCodeList(List<RewardElementData> itemList)
	{
		HashSet<string> hashSet = new HashSet<string>();
		for (int i = 0; i < itemList.Count; i++)
		{
			hashSet.Add(itemList[i].ItemCode);
		}
		return hashSet.ToList();
	}

	public List<string> GetItemCodeList()
	{
		List<string> list = new List<string>();
		SearchItemData(list, 1);
		return list;
	}

	private void SearchItemData(List<string> itemCodeList, int searchDepth)
	{
		if (searchDepth > 10)
		{
			return;
		}
		IEnumerator<string> itemCodeEnumerator = GetItemCodeEnumerator();
		while (itemCodeEnumerator.MoveNext())
		{
			string current = itemCodeEnumerator.Current;
			ItemGroupData data = GameRoot.Instance.ItemGroupDataContainer.GetData(current);
			if (data == null)
			{
				itemCodeList.Add(current);
			}
			else
			{
				data.SearchItemData(itemCodeList, searchDepth + 1);
			}
		}
	}

	public IEnumerator<string> GetItemCodeEnumerator()
	{
		if (SubDataList == null)
		{
			yield break;
		}
		for (int index = 0; index < SubDataList.Count; index++)
		{
			SubData subData = SubDataList[index];
			if (IsValidItemCode(subData.ItemCode))
			{
				yield return subData.ItemCode.ToString();
			}
		}
	}

	public static bool IsValidItemCode(HashString itemCode)
	{
		if (itemCode != null)
		{
			return itemCode.Length > 0;
		}
		return false;
	}

	public static List<RewardElementData> ExtractItemOnly(List<RewardElementData> itemList)
	{
		List<RewardElementData> list = new List<RewardElementData>();
		for (int i = 0; i < itemList.Count; i++)
		{
			RewardElementData rewardElementData = itemList[i];
			HashString itemCode = rewardElementData.ItemCode;
			if (GameRoot.Instance.ItemGroupDataContainer.GetData(itemCode) == null)
			{
				list.Add(rewardElementData);
			}
		}
		return list;
	}

	public static List<RewardElementData> ExtractItemGroup(List<RewardElementData> itemList, bool ignoreRate = false)
	{
		HashSet<string> extractedItemGroups = new HashSet<string>();
		return ExtractItemGroup(itemList, extractedItemGroups, ignoreRate);
	}

	public static List<RewardElementData> ExtractItemGroup(List<RewardElementData> itemList, HashSet<string> extractedItemGroups, bool ignoreRate)
	{
		List<RewardElementData> list = new List<RewardElementData>();
		for (int i = 0; i < itemList.Count; i++)
		{
			RewardElementData rewardElementData = itemList[i];
			HashString itemCode = rewardElementData.ItemCode;
			int itemRate = rewardElementData.ItemRate;
			int itemMin = rewardElementData.ItemMin;
			int itemMax = rewardElementData.ItemMax;
			ItemGroupData data = GameRoot.Instance.ItemGroupDataContainer.GetData(itemCode);
			if (data == null)
			{
				list.Add(rewardElementData);
			}
			else
			{
				if (extractedItemGroups.Contains(itemCode))
				{
					continue;
				}
				extractedItemGroups.Add(itemCode);
				List<RewardElementData> rewardItemList = data.GetRewardItemList();
				int num = 0;
				for (int j = 0; j < rewardItemList.Count; j++)
				{
					num += rewardItemList[j].ItemRate;
				}
				if (ignoreRate)
				{
					for (int k = 0; k < rewardItemList.Count; k++)
					{
						rewardItemList[k].ItemRate = 1000 / rewardItemList.Count;
						rewardItemList[k].ItemMin = rewardItemList[k].ItemMin * itemMin;
						rewardItemList[k].ItemMax = rewardItemList[k].ItemMax * itemMax;
					}
					list.AddRange(rewardItemList);
				}
				else if (num != 0)
				{
					for (int l = 0; l < rewardItemList.Count; l++)
					{
						rewardItemList[l].ItemRate = (int)((long)rewardItemList[l].ItemRate * 1000L / num);
					}
					for (int m = 0; m < rewardItemList.Count; m++)
					{
						rewardItemList[m].ItemRate = (int)((long)rewardItemList[m].ItemRate * (long)itemRate / 1000);
						rewardItemList[m].ItemMin = rewardItemList[m].ItemMin * itemMin;
						rewardItemList[m].ItemMax = rewardItemList[m].ItemMax * itemMax;
					}
					list.AddRange(rewardItemList);
				}
			}
		}
		return list;
	}

	public static List<RewardElementData> ExtractItemWithGroup(RewardElementData itemGroupElementData, out string desc, out string itemGroupCode)
	{
		ItemGroupData data = GameRoot.Instance.ItemGroupDataContainer.GetData(itemGroupElementData.ItemCode);
		if (data == null)
		{
			desc = null;
			itemGroupCode = null;
			return null;
		}
		desc = data.ItemGroupDesc;
		itemGroupCode = data.ItemGroupCode;
		List<RewardElementData> rewardItemList = data.GetRewardItemList();
		int num = 0;
		for (int i = 0; i < rewardItemList.Count; i++)
		{
			num += rewardItemList[i].ItemRate;
		}
		if (num == 0)
		{
			return null;
		}
		for (int j = 0; j < rewardItemList.Count; j++)
		{
			rewardItemList[j].ItemRate = (int)((long)rewardItemList[j].ItemRate * 1000L / num);
		}
		for (int k = 0; k < rewardItemList.Count; k++)
		{
			rewardItemList[k].ItemRate = (int)((long)rewardItemList[k].ItemRate * (long)itemGroupElementData.ItemRate / 1000);
			rewardItemList[k].ItemMin = rewardItemList[k].ItemMin * itemGroupElementData.ItemMin;
			rewardItemList[k].ItemMax = rewardItemList[k].ItemMax * itemGroupElementData.ItemMax;
		}
		List<RewardElementData> list = new List<RewardElementData>();
		list.AddRange(rewardItemList);
		return list;
	}
}
