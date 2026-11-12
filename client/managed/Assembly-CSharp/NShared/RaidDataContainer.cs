using System;
using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class RaidDataContainer : JitDataContainerDefault<RaidDataKey2, RaidData>
{
	public const string TABLENAME = "RaidTable";

	private JitDictionary<RaidType, HashSet<int>> _indexSets_by_RaidType = new JitDictionary<RaidType, HashSet<int>>();

	private JitDictionary<int, HashSet<int>> _levelSets_by_Index = new JitDictionary<int, HashSet<int>>();

	private JitDictionary<int, int> _map_for_SingleToMulti = new JitDictionary<int, int>();

	private JitDictionary<ChapterDungeonKey2, RaidData> _datas_by_ChapterDungeon = new JitDictionary<ChapterDungeonKey2, RaidData>();

	private JitDictionary<RaidType, List<RaidData>> _lists_by_Type = new JitDictionary<RaidType, List<RaidData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("RaidTable");

	public RaidDataContainer()
	{
		base.Packer.RegisterAndBind(_indexSets_by_RaidType);
		base.Packer.RegisterAndBind(_levelSets_by_Index);
		base.Packer.RegisterAndBind(_map_for_SingleToMulti);
		base.Packer.RegisterAndBind(_datas_by_ChapterDungeon);
		base.Packer.RegisterAndBind(_lists_by_Type);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (RaidData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_indexSets_by_RaidType.JsonLoad_AddToHashSetValue(item.Type, item.Index);
			_levelSets_by_Index.JsonLoad_AddToHashSetValue(item.Index, item.Level);
			ChapterDungeonKey2 key = new ChapterDungeonKey2(item.ChapterIndex, item.DungeonIndex);
			if (!_datas_by_ChapterDungeon.JsonLoad_Get().ContainsKey(key))
			{
				_datas_by_ChapterDungeon.JsonLoad_Get().Add(key, item);
			}
			if (item.SingleRaidIndex != 0 && !_map_for_SingleToMulti.JsonLoad_Get().ContainsKey(item.SingleRaidIndex))
			{
				_map_for_SingleToMulti.JsonLoad_Get().Add(item.SingleRaidIndex, item.Index);
			}
			_lists_by_Type.JsonLoad_AddToListValue(item.Type, item);
		}
	}

	public RaidData GetData(int index, int level)
	{
		return GetData(new RaidDataKey2(index, level));
	}

	public ICollection<int> GetRaidIndices(RaidType raidType)
	{
		return _indexSets_by_RaidType.GetOrDefault(raidType);
	}

	public ICollection<int> GetRaidLevels(int raidIndex)
	{
		return _levelSets_by_Index.GetOrDefault(raidIndex);
	}

	public IList<RaidData> GetRaidDataListByType(RaidType raidType)
	{
		return __Extension_Method_NShared_ReadonlyEmpty__.GetEmptyIfNull(_lists_by_Type.GetOrDefault(raidType));
	}

	public int GetRaidMinLevel(int raidIndex)
	{
		int num = int.MaxValue;
		ICollection<int> raidLevels = GetRaidLevels(raidIndex);
		if (raidLevels == null)
		{
			return num;
		}
		foreach (int item in raidLevels)
		{
			num = Math.Min(num, item);
		}
		return num;
	}

	public int GetRaidMaxLevel(int raidIndex)
	{
		int num = 1;
		ICollection<int> raidLevels = GetRaidLevels(raidIndex);
		if (raidLevels == null)
		{
			return num;
		}
		IEnumerator<int> enumerator = raidLevels.GetEnumerator();
		while (enumerator.MoveNext())
		{
			int current = enumerator.Current;
			num = Math.Max(num, current);
		}
		return num;
	}

	public RaidData GetRaidDataByDungeon(int chapterIndex, int dungeonIndex)
	{
		return _datas_by_ChapterDungeon.GetOrDefault(new ChapterDungeonKey2(chapterIndex, dungeonIndex));
	}

	public int GetMultiRaidIndexBySingleRaidIndex(int singleRaidIndex)
	{
		return _map_for_SingleToMulti.GetOrDefault(singleRaidIndex);
	}
}
