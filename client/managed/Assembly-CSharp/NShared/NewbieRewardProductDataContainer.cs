using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class NewbieRewardProductDataContainer : JitDataContainerDefault<int, NewbieRewardProductData>
{
	public const string TABLENAME = "NewbieRewardProductTable";

	public override TablePathDefine PathDefine => new TablePathDefine("NewbieRewardProductTable");

	public NewbieRewardProductData[] GetDataByRewardType(NewbieRewardType rewardType)
	{
		List<NewbieRewardProductData> list = new List<NewbieRewardProductData>();
		IList<NewbieRewardProductData> allData = GetAllData();
		if (allData == null)
		{
			return list.ToArray();
		}
		for (int i = 0; i < allData.Count; i++)
		{
			NewbieRewardProductData newbieRewardProductData = allData[i];
			if (newbieRewardProductData != null && newbieRewardProductData.RewardType == rewardType)
			{
				list.Add(newbieRewardProductData);
			}
		}
		return list.ToArray();
	}

	public NewbieRewardProductData GetDataByPayShopProductIndex(int payShopProductIndex)
	{
		IList<NewbieRewardProductData> allData = GetAllData();
		if (allData == null)
		{
			return null;
		}
		NewbieRewardProductData result = null;
		for (int i = 0; i < allData.Count; i++)
		{
			NewbieRewardProductData newbieRewardProductData = allData[i];
			if (newbieRewardProductData != null && newbieRewardProductData.PayShopProductIndex == payShopProductIndex)
			{
				result = newbieRewardProductData;
				break;
			}
		}
		return result;
	}

	public NewbieRewardProductData GetDataByGroupIndex(int groupIndex)
	{
		IList<NewbieRewardProductData> allData = GetAllData();
		if (allData == null)
		{
			return null;
		}
		NewbieRewardProductData result = null;
		for (int i = 0; i < allData.Count; i++)
		{
			NewbieRewardProductData newbieRewardProductData = allData[i];
			if (newbieRewardProductData != null && newbieRewardProductData.GroupIndex == groupIndex)
			{
				result = newbieRewardProductData;
				break;
			}
		}
		return result;
	}

	public List<NewbieRewardProductData> GetMissionAvailableDatas()
	{
		List<NewbieRewardProductData> list = new List<NewbieRewardProductData>();
		IList<NewbieRewardProductData> allData = GetAllData();
		if (allData == null)
		{
			return list;
		}
		for (int i = 0; i < allData.Count; i++)
		{
			NewbieRewardProductData newbieRewardProductData = allData[i];
			if (newbieRewardProductData != null && newbieRewardProductData.GroupIndex > 0)
			{
				list.Add(newbieRewardProductData);
			}
		}
		return list;
	}
}
