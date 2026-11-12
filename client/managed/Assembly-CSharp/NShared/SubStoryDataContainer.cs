using System.Collections.Generic;
using System.Linq;
using MessagePack;
using NShared.NJit;

namespace NShared;

public class SubStoryDataContainer : JitDataContainerDefault<ChapterDungeonKey2, SubStoryData>
{
	[MessagePackObject(false)]
	public class ChapterInfo
	{
		[Key(0)]
		public int CreatureIndex { get; set; }

		[Key(1)]
		public bool IsSubStoryOnly { get; set; }
	}

	public const string TABLENAME = "SubStoryTable";

	private JitDictionary<int, ChapterInfo> _infos_by_Chapter = new JitDictionary<int, ChapterInfo>();

	private JitRef<List<ChapterDungeonKey2>> _dungeons_for_UnlockCostume = new JitRef<List<ChapterDungeonKey2>>();

	private CreatureTagType _lastTagType;

	private List<SubStoryData> _lastTagTypeDataList = new List<SubStoryData>();

	public override TablePathDefine PathDefine => new TablePathDefine("SubStoryTable");

	public SubStoryDataContainer()
	{
		base.Packer.RegisterAndBind(_infos_by_Chapter);
		base.Packer.RegisterAndBind(_dungeons_for_UnlockCostume);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (SubStoryData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (!_infos_by_Chapter.JsonLoad_Get().TryGetValue(item.ChapterIndex, out var value))
			{
				value = new ChapterInfo();
				value.IsSubStoryOnly = true;
				_infos_by_Chapter.JsonLoad_Get().Add(item.ChapterIndex, value);
			}
			value.CreatureIndex = item.CreatureIndex;
			if (item.NoShowNodeInfo)
			{
				value.IsSubStoryOnly = false;
			}
		}
		_dungeons_for_UnlockCostume.JsonLoad_Set((from v in base.BaseList.JsonLoad_GetOrNew()
			where v.UnlockCostumeIndex != 0
			select new ChapterDungeonKey2(v.ChapterIndex, v.DungeonIndex)).Distinct().ToList());
	}

	public SubStoryData GetData(int chapterIndex, int dungeonIndex)
	{
		return GetData(new ChapterDungeonKey2(chapterIndex, dungeonIndex));
	}

	public IList<ChapterDungeonKey2> GetDungeons_for_UnlockCostume()
	{
		return _dungeons_for_UnlockCostume.Eval();
	}

	public bool IsOnlySubStoryChapter(int chapterIndex)
	{
		return _infos_by_Chapter.GetOrDefault(chapterIndex)?.IsSubStoryOnly ?? false;
	}

	public bool IsSubStoryChapter(int chapterIndex)
	{
		return IsOnlySubStoryChapter(chapterIndex);
	}

	public int GetCreatureIndexForChapter(int chapterIndex)
	{
		return _infos_by_Chapter.GetOrDefault(chapterIndex)?.CreatureIndex ?? 0;
	}

	public List<SubStoryData> GetListForHero()
	{
		List<SubStoryData> list = new List<SubStoryData>();
		foreach (SubStoryData allDatum in GetAllData())
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(allDatum.CreatureIndex);
			if (data != null && data.TagType != CreatureTagType.None && data.TagType != CreatureTagType.End)
			{
				list.Add(allDatum);
			}
		}
		return list;
	}

	public List<SubStoryData> GetOnlySubStoryDataList()
	{
		List<SubStoryData> list = new List<SubStoryData>();
		foreach (SubStoryData allDatum in GetAllData())
		{
			if (IsOnlySubStoryChapter(allDatum.ChapterIndex))
			{
				list.Add(allDatum);
			}
		}
		return list;
	}

	public List<SubStoryData> GetListForCreatureTagType(CreatureTagType reqTagType)
	{
		if (_lastTagType != CreatureTagType.None && _lastTagType == reqTagType)
		{
			return _lastTagTypeDataList;
		}
		_lastTagType = reqTagType;
		_lastTagTypeDataList.Clear();
		foreach (SubStoryData allDatum in GetAllData())
		{
			if (IsOnlySubStoryChapter(allDatum.ChapterIndex))
			{
				CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(allDatum.CreatureIndex);
				if (data != null && data.TagType == reqTagType)
				{
					_lastTagTypeDataList.Add(allDatum);
				}
			}
		}
		return _lastTagTypeDataList;
	}
}
