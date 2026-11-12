using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class SubQuestDataContainer : JitDataContainerDefault<SubQuestDataKey, SubQuestData>
{
	public const string TABLENAME = "SubQuestTable";

	private JitDictionary<int, List<SubQuestData>> _sortedLists_by_QuestIndex = new JitDictionary<int, List<SubQuestData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("SubQuestTable");

	public SubQuestDataContainer()
	{
		base.Packer.RegisterAndBind(_sortedLists_by_QuestIndex);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (SubQuestData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_sortedLists_by_QuestIndex.JsonLoad_AddToListValue(item.QuestIndex, item);
		}
		_sortedLists_by_QuestIndex.JsonLoad_SortListValue((SubQuestData lhs, SubQuestData rhs) => rhs.Step.CompareTo(lhs.Step));
	}

	public IList<SubQuestData> GetListByQuestIndex(int index)
	{
		return __Extension_Method_NShared_ReadonlyEmpty__.GetEmptyIfNull(_sortedLists_by_QuestIndex.GetOrDefault(index));
	}

	public SubQuestData GetSubQuestData(int questIndex, int step)
	{
		return GetData(new SubQuestDataKey(questIndex, step));
	}

	public SubQuestData GetSubQuestData(int questIndex)
	{
		return GetSubQuestData(questIndex, 1);
	}

	public int GetLastStepByQuestIndex(int questIndex)
	{
		List<SubQuestData> orDefault = _sortedLists_by_QuestIndex.GetOrDefault(questIndex);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(orDefault))
		{
			return -1;
		}
		return orDefault[0].Step;
	}
}
