using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class TowerDataContainer : JitDataContainerDefault<int, TowerData>
{
	public const string TABLENAME = "TowerTable";

	public override TablePathDefine PathDefine => new TablePathDefine("TowerTable");

	public TemporaryList<TowerData> GetTowers(TowerType type)
	{
		TemporaryList<TowerData> result = default(TemporaryList<TowerData>);
		foreach (TowerData allDatum in GetAllData())
		{
			if (allDatum.Type == type)
			{
				result.Add(allDatum);
			}
		}
		return result;
	}

	public int[] GetTowerIndices(TowerType type)
	{
		List<int> list = new List<int>();
		foreach (TowerData allDatum in GetAllData())
		{
			if (allDatum.Type == type)
			{
				list.Add(allDatum.TowerIndex);
			}
		}
		return list.ToArray();
	}

	public int GetTowerIndexByType(TowerType type)
	{
		IList<TowerData> allData = GetAllData();
		TowerData towerData = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allData); i++)
		{
			towerData = allData[i];
			if (towerData != null && type == towerData.Type)
			{
				return towerData.TowerIndex;
			}
		}
		return 0;
	}
}
