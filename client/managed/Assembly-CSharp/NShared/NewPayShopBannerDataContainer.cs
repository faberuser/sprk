using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class NewPayShopBannerDataContainer : JitDataContainerDefault<int, NewPayShopBannerData>
{
	public const string TABLENAME = "NewPayShopBannerTable";

	public override TablePathDefine PathDefine => new TablePathDefine("NewPayShopBannerTable");

	public List<NewPayShopBannerData> GetBannerDataByPlayerType(PayShopUserGroup type)
	{
		IList<NewPayShopBannerData> allData = GetAllData();
		List<NewPayShopBannerData> list = new List<NewPayShopBannerData>();
		for (int i = 0; i < allData.Count; i++)
		{
			if (allData[i].Group == type)
			{
				list.Add(allData[i]);
			}
		}
		list.Sort((NewPayShopBannerData l, NewPayShopBannerData r) => l.Index.CompareTo(r.Index));
		return list;
	}
}
