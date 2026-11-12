using System;
using System.Collections.Generic;
using MessagePack;
using NShared.NJit;

namespace NShared;

public class GuildRaidDungeonDataContainer : JitDataContainerDefault<GuildRaidDungeonDataKey, GuildRaidDungeonData>
{
	[MessagePackObject(false)]
	[JitKey]
	public struct GuildRaidStepKey
	{
		[Key(0)]
		public int ChapterIndex;

		[Key(1)]
		public int Step;

		public GuildRaidStepKey(int chapterIndex, int step)
		{
			ChapterIndex = chapterIndex;
			Step = step;
		}

		public bool Equals(GuildRaidStepKey other)
		{
			if (ChapterIndex == other.ChapterIndex)
			{
				return Step == other.Step;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return JitOption.BuildHash(ChapterIndex, Step);
		}

		public override string ToString()
		{
			return $"{GetType().Name}({ChapterIndex}, {Step})";
		}
	}

	public const string TABLENAME = "GuildRaidDungeonTable";

	private JitDictionary<int, List<GuildRaidDungeonData>> _guildRaidDungeonDataDic = new JitDictionary<int, List<GuildRaidDungeonData>>();

	private JitDictionary<GuildRaidStepKey, int> _maxDungeonIndices = new JitDictionary<GuildRaidStepKey, int>();

	private JitRef<Dictionary<int, int>> _chapterLastStepDic = new JitRef<Dictionary<int, int>>();

	private int _totalStep;

	public override TablePathDefine PathDefine => new TablePathDefine("GuildRaidDungeonTable");

	public int TotalStep => _totalStep;

	public GuildRaidDungeonDataContainer()
	{
		base.Packer.RegisterAndBind(_guildRaidDungeonDataDic);
		base.Packer.RegisterAndBind(_maxDungeonIndices);
		base.Packer.RegisterAndBind(_chapterLastStepDic);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (GuildRaidDungeonData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_guildRaidDungeonDataDic.JsonLoad_AddToListValue(item.ChapterIndex, item);
		}
		_guildRaidDungeonDataDic.JsonLoad_SortListValue((GuildRaidDungeonData lhs, GuildRaidDungeonData rhs) => lhs.Step.CompareTo(rhs.Step));
		foreach (GuildRaidDungeonData item2 in base.BaseList.JsonLoad_GetOrNew())
		{
			GuildRaidStepKey key = new GuildRaidStepKey(item2.ChapterIndex, item2.Step);
			if (!_maxDungeonIndices.JsonLoad_Get().TryGetValue(key, out var value))
			{
				value = 0;
			}
			_maxDungeonIndices.JsonLoad_Get()[key] = Math.Max(value, item2.DungeonIndex);
		}
		Dictionary<int, int> dictionary = _chapterLastStepDic.JsonLoad_GetOrNew();
		foreach (GuildRaidDungeonData item3 in base.BaseList.JsonLoad_GetOrNew())
		{
			if (!dictionary.TryGetValue(item3.ChapterIndex, out var value2))
			{
				value2 = 0;
			}
			dictionary[item3.ChapterIndex] = Math.Max(value2, item3.Step);
		}
	}

	protected override void OnEndLoad_Final()
	{
		_totalStep = 0;
		foreach (KeyValuePair<int, int> item in _chapterLastStepDic.Eval())
		{
			_totalStep += item.Value;
		}
	}

	public GuildRaidDungeonData GetData(int chapterIndex, int dungeonIndex, int step)
	{
		return GetData(new GuildRaidDungeonDataKey(chapterIndex, dungeonIndex, step));
	}

	public int GetChapterLastStep(int chapterIndex)
	{
		if (!_chapterLastStepDic.Eval().ContainsKey(chapterIndex))
		{
			return 0;
		}
		return _chapterLastStepDic.Eval()[chapterIndex];
	}

	public IList<GuildRaidDungeonData> GetGuildRaidDungeonDataList(int chapterIndex)
	{
		return _guildRaidDungeonDataDic.GetOrDefault(chapterIndex);
	}
}
