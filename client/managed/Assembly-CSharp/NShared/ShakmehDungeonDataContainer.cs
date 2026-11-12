using System;
using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class ShakmehDungeonDataContainer : JitDataContainerDefault<ShakmehDungeonKey, ShakmehDungeonData>
{
	public const string TABLENAME = "ShakmehDungeonTable";

	private JitDictionary<int, List<int>> _levelLists_by_ShakmehIndex = new JitDictionary<int, List<int>>();

	private JitDictionary<int, ShakmehDungeonData> _datas_by_DungeonIndex = new JitDictionary<int, ShakmehDungeonData>();

	public override TablePathDefine PathDefine => new TablePathDefine("ShakmehDungeonTable");

	public ShakmehDungeonDataContainer()
	{
		base.Packer.RegisterAndBind(_levelLists_by_ShakmehIndex);
		base.Packer.RegisterAndBind(_datas_by_DungeonIndex);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (ShakmehDungeonData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_levelLists_by_ShakmehIndex.JsonLoad_AddToListValue(item.ShakmehIndex, item.Level);
			_datas_by_DungeonIndex.JsonLoad_AddIfNotExist(item.DungeonIndex, item);
		}
	}

	public ShakmehDungeonData GetData(int index, int level)
	{
		return GetData(new ShakmehDungeonKey(index, level));
	}

	public ICollection<int> GetShakmehLevels(int shakmehIndex)
	{
		return _levelLists_by_ShakmehIndex.GetOrDefault(shakmehIndex);
	}

	public int GetMinLevel(int raidIndex)
	{
		int num = 1;
		ICollection<int> shakmehLevels = GetShakmehLevels(raidIndex);
		if (shakmehLevels == null)
		{
			return num;
		}
		IEnumerator<int> enumerator = shakmehLevels.GetEnumerator();
		while (enumerator.MoveNext())
		{
			int current = enumerator.Current;
			num = Math.Min(num, current);
		}
		return num;
	}

	public int GetMaxLevel(int shakmehIndex)
	{
		int num = 1;
		ICollection<int> shakmehLevels = GetShakmehLevels(shakmehIndex);
		if (shakmehLevels == null)
		{
			return num;
		}
		IEnumerator<int> enumerator = shakmehLevels.GetEnumerator();
		while (enumerator.MoveNext())
		{
			int current = enumerator.Current;
			num = Math.Max(num, current);
		}
		return num;
	}

	public ShakmehDungeonData GetDataByDungeonIndex(int dungeonIndex)
	{
		return _datas_by_DungeonIndex.GetOrDefault(dungeonIndex);
	}
}
