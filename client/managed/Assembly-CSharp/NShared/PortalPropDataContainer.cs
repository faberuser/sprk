using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class PortalPropDataContainer : JitDataContainerListOnly<PortalPropData>
{
	public const string TABLENAME = "PortalPropTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PortalPropTable");

	public List<PortalPropData> GetPropDataOfPortal()
	{
		List<PortalPropData> list = new List<PortalPropData>();
		IList<PortalPropData> allData = GetAllData();
		for (int i = 0; i < allData.Count; i++)
		{
			if (allData[i].ChapterIndex == 0 && allData[i].DungeonIndex == 0)
			{
				list.Add(allData[i]);
			}
		}
		return list;
	}

	public List<PortalPropData> GetPropDataOfPortalPopup(int ChapterIndex, int DungeonIndex)
	{
		List<PortalPropData> list = new List<PortalPropData>();
		IList<PortalPropData> allData = GetAllData();
		for (int i = 0; i < allData.Count; i++)
		{
			if (allData[i].ChapterIndex == ChapterIndex && allData[i].DungeonIndex == DungeonIndex)
			{
				list.Add(allData[i]);
			}
		}
		return list;
	}
}
