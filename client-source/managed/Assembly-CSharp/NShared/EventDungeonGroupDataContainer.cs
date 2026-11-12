using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class EventDungeonGroupDataContainer : JitDataContainerDefault<EventDungeonGroupKey, EventDungeonGroupData>
{
	public const string TABLENAME = "EventDungeonGroupTable";

	private JitDictionary<int, List<EventDungeonGroupData>> _dicByChapter = new JitDictionary<int, List<EventDungeonGroupData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("EventDungeonGroupTable");

	public EventDungeonGroupDataContainer()
	{
		base.Packer.RegisterAndBind(_dicByChapter);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (EventDungeonGroupData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_dicByChapter.JsonLoad_AddToListValue(item.ChapterIndex, item);
		}
	}

	public EventDungeonGroupData GetData(int chapterIndex, int groupIndex)
	{
		return GetData(new EventDungeonGroupKey(chapterIndex, groupIndex));
	}

	public IList<EventDungeonGroupData> GetDatasByChapterIndex(int chapterIndex)
	{
		return _dicByChapter.GetOrDefault(chapterIndex);
	}
}
