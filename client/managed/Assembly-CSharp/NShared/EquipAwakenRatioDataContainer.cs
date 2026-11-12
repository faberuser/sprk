using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class EquipAwakenRatioDataContainer : JitDataContainerDefault<EquipAwakenRatioDataKey, EquipAwakenRatioData>
{
	public const string TABLENAME = "EquipAwakenRatioTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EquipAwakenRatioTable");

	public EquipAwakenRatioData GetData(int star, int awakenPoint)
	{
		return GetData(new EquipAwakenRatioDataKey(star, awakenPoint));
	}

	public int GetMaxAwakenPoint(int star)
	{
		IList<EquipAwakenRatioData> allData = GetAllData();
		int num = 0;
		foreach (EquipAwakenRatioData item in allData)
		{
			if (item.Star == star && item.AwakenPoint > num)
			{
				num = item.AwakenPoint;
			}
		}
		return num;
	}
}
