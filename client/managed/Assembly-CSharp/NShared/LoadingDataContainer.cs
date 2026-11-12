using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using NShared.NJit;

namespace NShared;

public class LoadingDataContainer : JitDataContainerDefault<int, LoadingData>
{
	[MessagePackObject(false)]
	public class NormalLoadingInfo
	{
		[Key(0)]
		public int TotalRate { get; set; }

		[Key(1)]
		public List<int> DataKeyList { get; set; }
	}

	[MessagePackObject(false)]
	public class BattleLoadingInfo
	{
		[Key(0)]
		public int DungeonIndex_Begin { get; set; }

		[Key(1)]
		public int DungeonIndex_End { get; set; }

		[Key(2)]
		public int TotalRate { get; set; }

		[Key(3)]
		public List<int> DataKeyList { get; set; }
	}

	public const string TABLENAME = "LoadingTable";

	private JitDictionary<ChapterDungeonKey2, LoadingData> _map_for_StartBattle = new JitDictionary<ChapterDungeonKey2, LoadingData>();

	private JitDictionary<ChapterDungeonKey2, LoadingData> _map_for_EndBattle = new JitDictionary<ChapterDungeonKey2, LoadingData>();

	private JitDictionary<LoadingData.EGroupType, NormalLoadingInfo> _infoDic_for_NormalLoading = new JitDictionary<LoadingData.EGroupType, NormalLoadingInfo>();

	private JitDictionary<int, List<BattleLoadingInfo>> _infoDic_for_BattleLoading = new JitDictionary<int, List<BattleLoadingInfo>>();

	public override TablePathDefine PathDefine => new TablePathDefine("LoadingTable");

	public LoadingDataContainer()
	{
		base.Packer.RegisterAndBind(_map_for_StartBattle);
		base.Packer.RegisterAndBind(_map_for_EndBattle);
		base.Packer.RegisterAndBind(_infoDic_for_NormalLoading);
		base.Packer.RegisterAndBind(_infoDic_for_BattleLoading);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (LoadingData item in from v in base.BaseList.JsonLoad_GetOrNew()
			where v.GroupType == LoadingData.EGroupType.StartBattle
			where v.GetGroupArgInt(0) != 0 && v.GetGroupArgInt(1) != 0
			select v)
		{
			int groupArgInt = item.GetGroupArgInt(0);
			int groupArgInt2 = item.GetGroupArgInt(1);
			ChapterDungeonKey2 key = new ChapterDungeonKey2(groupArgInt, groupArgInt2);
			if (!_map_for_StartBattle.JsonLoad_Get().ContainsKey(key))
			{
				_map_for_StartBattle.JsonLoad_Get().Add(key, item);
			}
		}
		foreach (LoadingData item2 in from v in base.BaseList.JsonLoad_GetOrNew()
			where v.GroupType == LoadingData.EGroupType.EndBattle
			where v.GetGroupArgInt(0) != 0 && v.GetGroupArgInt(1) != 0
			select v)
		{
			int groupArgInt3 = item2.GetGroupArgInt(0);
			int groupArgInt4 = item2.GetGroupArgInt(1);
			ChapterDungeonKey2 key2 = new ChapterDungeonKey2(groupArgInt3, groupArgInt4);
			if (!_map_for_EndBattle.JsonLoad_Get().ContainsKey(key2))
			{
				_map_for_EndBattle.JsonLoad_Get().Add(key2, item2);
			}
		}
		foreach (LoadingData item3 in from v in base.BaseList.JsonLoad_GetOrNew()
			where v.GroupType == LoadingData.EGroupType.StartGame || v.GroupType == LoadingData.EGroupType.Default
			where v.ShowRate > 0
			select v)
		{
			NormalLoadingInfo normalLoadingInfo = _infoDic_for_NormalLoading.JsonLoad_ValueGetOrNew(item3.GroupType);
			normalLoadingInfo.TotalRate += item3.ShowRate;
			normalLoadingInfo.DataKeyList = normalLoadingInfo.DataKeyList ?? new List<int>();
			normalLoadingInfo.DataKeyList.Add(item3.Index);
		}
		foreach (LoadingData item4 in from v in base.BaseList.JsonLoad_GetOrNew()
			where v.GroupType == LoadingData.EGroupType.Battle
			where v.ShowRate > 0
			select v)
		{
			int groupArgInt5 = item4.GetGroupArgInt(0);
			int startDungeonIndex = item4.GetGroupArgInt(1);
			int endDungeonIndex = item4.GetGroupArgInt(2);
			if (groupArgInt5 != 0 && startDungeonIndex != 0 && endDungeonIndex != 0)
			{
				BattleLoadingInfo battleLoadingInfo = _infoDic_for_BattleLoading.JsonLoad_ValueGetOrNew(groupArgInt5).Find((BattleLoadingInfo v) => v.DungeonIndex_Begin == startDungeonIndex && v.DungeonIndex_End == endDungeonIndex);
				if (battleLoadingInfo == null)
				{
					battleLoadingInfo = new BattleLoadingInfo();
					battleLoadingInfo.DungeonIndex_Begin = startDungeonIndex;
					battleLoadingInfo.DungeonIndex_End = endDungeonIndex;
				}
				battleLoadingInfo.TotalRate += item4.ShowRate;
				battleLoadingInfo.DataKeyList = battleLoadingInfo.DataKeyList ?? new List<int>();
				battleLoadingInfo.DataKeyList.Add(item4.Index);
			}
		}
	}

	public LoadingData GetLoadingData(LoadingData.EGroupType groupType)
	{
		NormalLoadingInfo orDefault = _infoDic_for_NormalLoading.GetOrDefault(groupType);
		if (orDefault == null)
		{
			return GetFirstData();
		}
		LoadingData randomLoadingData = GetRandomLoadingData(orDefault.TotalRate, orDefault.DataKeyList);
		if (randomLoadingData != null)
		{
			return randomLoadingData;
		}
		return GetFirstData();
	}

	public LoadingData GetLoadingDataByStartChapterDungeon(int chapterIndex, int dungeonIndex)
	{
		LoadingData orDefault = _map_for_StartBattle.GetOrDefault(new ChapterDungeonKey2(chapterIndex, dungeonIndex));
		if (orDefault != null)
		{
			return orDefault;
		}
		orDefault = GetLoadingDataByBattle(chapterIndex, dungeonIndex);
		if (orDefault != null)
		{
			return orDefault;
		}
		return GetLoadingData(LoadingData.EGroupType.Default);
	}

	public LoadingData GetLoadingDataByEndChapterDungeon(int chapterIndex, int dungeonIndex)
	{
		LoadingData orDefault = _map_for_EndBattle.GetOrDefault(new ChapterDungeonKey2(chapterIndex, dungeonIndex));
		if (orDefault != null)
		{
			return orDefault;
		}
		orDefault = GetLoadingDataByBattle(chapterIndex, dungeonIndex);
		if (orDefault != null)
		{
			return orDefault;
		}
		return GetLoadingData(LoadingData.EGroupType.Default);
	}

	private LoadingData GetLoadingDataByBattle(int chapterIndex, int dungeonIndex)
	{
		List<BattleLoadingInfo> orDefault = _infoDic_for_BattleLoading.GetOrDefault(chapterIndex);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(orDefault))
		{
			return null;
		}
		BattleLoadingInfo battleLoadingInfo = orDefault.Find((BattleLoadingInfo v) => dungeonIndex >= v.DungeonIndex_Begin && dungeonIndex <= v.DungeonIndex_End);
		if (battleLoadingInfo == null)
		{
			return null;
		}
		return GetRandomLoadingData(battleLoadingInfo.TotalRate, battleLoadingInfo.DataKeyList);
	}

	private LoadingData GetRandomLoadingData(int TotalRate, IList<int> dataKeyList)
	{
		if (TotalRate <= 0)
		{
			return null;
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(dataKeyList))
		{
			return null;
		}
		int num = new Random(DateTime.Now.Millisecond).Next(TotalRate);
		int num2 = 0;
		int num3 = 0;
		foreach (int dataKey in dataKeyList)
		{
			LoadingData data = GetData(dataKey);
			if (data != null)
			{
				num3 += data.ShowRate;
				if (num >= num2 && num < num3)
				{
					return data;
				}
				num2 += data.ShowRate;
			}
		}
		return null;
	}
}
