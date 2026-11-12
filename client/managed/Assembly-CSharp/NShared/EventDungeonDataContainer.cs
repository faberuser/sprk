using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class EventDungeonDataContainer : JitDataContainerDefault<EventDungeonKey, EventDungeonData>
{
	public const string TABLENAME = "EventDungeonTable";

	private JitDictionary<int, List<EventDungeonData>> _dicByChapter = new JitDictionary<int, List<EventDungeonData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("EventDungeonTable");

	public EventDungeonDataContainer()
	{
		base.Packer.RegisterAndBind(_dicByChapter);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (EventDungeonData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_dicByChapter.JsonLoad_AddToListValue(item.ChapterIndex, item);
		}
		_dicByChapter.JsonLoad_SortListValue((EventDungeonData lhs, EventDungeonData rhs) => lhs.ShowIndex.CompareTo(rhs.ShowIndex));
	}

	public IList<EventDungeonData> GetDatasByChapterIndex(int chapterIndex)
	{
		return _dicByChapter.GetOrDefault(chapterIndex);
	}

	public EventDungeonData GetData(int chapterIndex, int dungeonIndex)
	{
		return GetData(new EventDungeonKey(chapterIndex, dungeonIndex));
	}

	public Dictionary<CurrencyType, int> GetEventDungeonRewardPointDatas(DungeonDifficultyType dungeonDifficulty, int chapterIndex, int dungeonIndex)
	{
		EventDungeonData data = GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			return null;
		}
		int value = 0;
		int value2 = 0;
		switch (dungeonDifficulty)
		{
		case DungeonDifficultyType.Easy:
			value = data.EventDungeonPointAmount_Easy;
			value2 = data.EventDungeonPoint2Amount_Easy;
			break;
		case DungeonDifficultyType.Normal:
			value = data.EventDungeonPointAmount_Normal;
			value2 = data.EventDungeonPoint2Amount_Normal;
			break;
		case DungeonDifficultyType.Hard:
			value = data.EventDungeonPointAmount_Hard;
			value2 = data.EventDungeonPoint2Amount_Hard;
			break;
		case DungeonDifficultyType.Hell:
			value = data.EventDungeonPointAmount_Hell;
			value2 = data.EventDungeonPoint2Amount_Hell;
			break;
		}
		return new Dictionary<CurrencyType, int>
		{
			{
				CurrencyType.EventDungeonPoint,
				value
			},
			{
				CurrencyType.EventDungeonPoint2,
				value2
			}
		};
	}
}
