using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class ClearMissionProductDataContainer : JitDataContainerDefault<int, ClearMissionProductData>
{
	public const string TABLENAME = "ClearMissionProductTable";

	public override TablePathDefine PathDefine => new TablePathDefine("ClearMissionProductTable");

	public ClearMissionProductData[] GetDataByRewardType(ClearMissionType missionType)
	{
		List<ClearMissionProductData> list = new List<ClearMissionProductData>();
		IList<ClearMissionProductData> allData = GetAllData();
		if (allData == null)
		{
			return list.ToArray();
		}
		for (int i = 0; i < allData.Count; i++)
		{
			ClearMissionProductData clearMissionProductData = allData[i];
			if (clearMissionProductData != null && clearMissionProductData.MissionType == missionType)
			{
				list.Add(clearMissionProductData);
			}
		}
		return list.ToArray();
	}

	public ClearMissionProductData GetDataByPayShopProductIndex(int payShopProductIndex)
	{
		IList<ClearMissionProductData> allData = GetAllData();
		if (allData == null)
		{
			return null;
		}
		ClearMissionProductData result = null;
		for (int i = 0; i < allData.Count; i++)
		{
			ClearMissionProductData clearMissionProductData = allData[i];
			if (clearMissionProductData != null && clearMissionProductData.PayShopProductIndex == payShopProductIndex)
			{
				result = clearMissionProductData;
				break;
			}
		}
		return result;
	}

	public ClearMissionProductData GetDataByGroupIndex(int productGroupIndex)
	{
		IList<ClearMissionProductData> allData = GetAllData();
		if (allData == null)
		{
			return null;
		}
		ClearMissionProductData result = null;
		for (int i = 0; i < allData.Count; i++)
		{
			ClearMissionProductData clearMissionProductData = allData[i];
			if (clearMissionProductData != null && clearMissionProductData.ProductGroupIndex == productGroupIndex)
			{
				result = clearMissionProductData;
				break;
			}
		}
		return result;
	}

	public List<ClearMissionProductData> GetMissionAvailableDatas()
	{
		List<ClearMissionProductData> list = new List<ClearMissionProductData>();
		IList<ClearMissionProductData> allData = GetAllData();
		if (allData == null)
		{
			return list;
		}
		for (int i = 0; i < allData.Count; i++)
		{
			ClearMissionProductData clearMissionProductData = allData[i];
			if (clearMissionProductData != null && clearMissionProductData.ProductGroupIndex > 0)
			{
				list.Add(clearMissionProductData);
			}
		}
		return list;
	}
}
