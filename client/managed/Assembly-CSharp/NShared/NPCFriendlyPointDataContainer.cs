using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class NPCFriendlyPointDataContainer : JitDataContainerDefault<NPCFriendlyPointDataKey, NPCFriendlyPointData>
{
	public const string TABLENAME = "NPCFriendlyPointTable";

	public override TablePathDefine PathDefine => new TablePathDefine("NPCFriendlyPointTable");

	public NPCFriendlyPointData GetData(int heroIndex, int step)
	{
		return GetData(new NPCFriendlyPointDataKey(heroIndex, step));
	}

	public List<NPCFriendlyPointData> GetPointDataList(int heroIndex)
	{
		List<NPCFriendlyPointData> list = new List<NPCFriendlyPointData>();
		foreach (NPCFriendlyPointData allDatum in GetAllData())
		{
			if (allDatum.HeroIndex == heroIndex)
			{
				list.Add(allDatum);
			}
		}
		return list;
	}
}
