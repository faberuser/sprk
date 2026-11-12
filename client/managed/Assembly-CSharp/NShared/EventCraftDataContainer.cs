using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class EventCraftDataContainer : JitDataContainerDefault<int, EventCraftData>
{
	public const string TABLENAME = "EventCraftTable";

	private JitDictionary<string, List<EventCraftData>> _dicEventCraftByGroup = new JitDictionary<string, List<EventCraftData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("EventCraftTable");

	public EventCraftDataContainer()
	{
		base.Packer.RegisterAndBind(_dicEventCraftByGroup);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (EventCraftData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_dicEventCraftByGroup.JsonLoad_AddToListValue(item.ShowGroup, item);
		}
	}

	public IList<EventCraftData> GetDatasByGroup(string group)
	{
		return _dicEventCraftByGroup.GetOrDefault(group);
	}
}
