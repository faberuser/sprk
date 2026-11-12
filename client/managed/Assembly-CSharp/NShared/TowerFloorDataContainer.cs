using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class TowerFloorDataContainer : JitDataContainerDefault<TowerFloorKey, TowerFloorData>
{
	public const string TABLENAME = "TowerFloorTable";

	public override TablePathDefine PathDefine => new TablePathDefine("TowerFloorTable");

	public TowerFloorData GetData(int towerIndex, int floor)
	{
		return GetData(new TowerFloorKey(towerIndex, floor));
	}

	public TowerFloorData GetTowerDataByDungeon(int chapterIndex, int dungeonIndex)
	{
		foreach (TowerFloorData allDatum in GetAllData())
		{
			if (allDatum.ChapterIndex == chapterIndex && allDatum.DungeonIndex == dungeonIndex)
			{
				return allDatum;
			}
		}
		return null;
	}

	public int GetTowerIndex(int chapterIndex, int dungeonIndex)
	{
		foreach (TowerFloorData allDatum in GetAllData())
		{
			if (allDatum.ChapterIndex == chapterIndex && allDatum.DungeonIndex == dungeonIndex)
			{
				return allDatum.TowerIndex;
			}
		}
		return -1;
	}

	public int GetFirstFloor(int towerIndex)
	{
		foreach (TowerFloorData allDatum in GetAllData())
		{
			if (allDatum.TowerIndex == towerIndex)
			{
				return allDatum.Floor;
			}
		}
		return 1;
	}

	public List<TowerFloorData> GetFloors(int towerIndex)
	{
		List<TowerFloorData> list = new List<TowerFloorData>();
		foreach (TowerFloorData allDatum in GetAllData())
		{
			if (allDatum.TowerIndex == towerIndex)
			{
				list.Add(allDatum);
			}
		}
		return list;
	}

	public int GetFloorCount(int towerIndex)
	{
		IList<TowerFloorData> allData = GetAllData();
		int num = 0;
		foreach (TowerFloorData item in allData)
		{
			if (item.TowerIndex == towerIndex)
			{
				num++;
			}
		}
		return num;
	}
}
