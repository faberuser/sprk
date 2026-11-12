using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class EventForgeDataContainer : JitDataContainerDefault<int, EventForgeData>
{
	public const string TABLENAME = "EventForgeTable";

	private JitDictionary<string, int> _groupIndices_by_ItemCode = new JitDictionary<string, int>();

	public override TablePathDefine PathDefine => new TablePathDefine("EventForgeTable");

	public EventForgeDataContainer()
	{
		base.Packer.RegisterAndBind(_groupIndices_by_ItemCode);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (EventForgeData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_groupIndices_by_ItemCode.JsonLoad_AddIfNotExist(item.ItemCode, item.EnchatItemGroup);
		}
	}

	public EventForgeData GetDataByLevel(int group, int level)
	{
		IList<EventForgeData> allData = GetAllData();
		if (allData == null)
		{
			return null;
		}
		EventForgeData eventForgeData = null;
		for (int i = 0; i < allData.Count; i++)
		{
			eventForgeData = allData[i];
			if (eventForgeData != null && eventForgeData.EnchatItemGroup == group && eventForgeData.EnchantLevel == level)
			{
				return eventForgeData;
			}
		}
		return null;
	}

	public List<EventForgeData> GetDataListByGroup(int group)
	{
		IList<EventForgeData> allData = GetAllData();
		if (allData == null)
		{
			return null;
		}
		List<EventForgeData> list = new List<EventForgeData>();
		list.Clear();
		EventForgeData eventForgeData = null;
		for (int i = 0; i < allData.Count; i++)
		{
			eventForgeData = allData[i];
			if (eventForgeData != null && eventForgeData.EnchatItemGroup == group)
			{
				list.Add(eventForgeData);
			}
		}
		return list;
	}

	public int GetGroupByItemCode(string itemCode)
	{
		return _groupIndices_by_ItemCode.GetOrDefault(itemCode);
	}
}
