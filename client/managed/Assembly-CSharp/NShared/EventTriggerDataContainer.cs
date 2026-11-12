using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class EventTriggerDataContainer : JitDataContainerDefault<int, EventTriggerData>
{
	public const string TABLENAME = "EventTriggerTable";

	private JitDictionary<string, List<EventTriggerData>> _lists_by_EventType = new JitDictionary<string, List<EventTriggerData>>();

	private JitRef<List<EventTriggerData>> _list_of_MainScenario = new JitRef<List<EventTriggerData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("EventTriggerTable");

	public EventTriggerDataContainer()
	{
		base.Packer.RegisterAndBind(_lists_by_EventType);
		base.Packer.RegisterAndBind(_list_of_MainScenario);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (EventTriggerData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (!String_Extension_Method.IsNullOrEmpty(item.EventType))
			{
				_lists_by_EventType.JsonLoad_AddToListValue(item.EventType, item);
			}
			if (item.Category == EventTriggerCategory.MainScenario)
			{
				_list_of_MainScenario.JsonLoad_GetOrNew().Add(item);
			}
		}
		_list_of_MainScenario.JsonLoad_GetOrNew().Sort((EventTriggerData lhs, EventTriggerData rhs) => lhs.Index.CompareTo(rhs.Index));
	}

	public IList<EventTriggerData> GetDataByEventType(string eventType)
	{
		return _lists_by_EventType.GetOrDefault(eventType);
	}

	public IList<EventTriggerData> GetMainScenarioList()
	{
		return _list_of_MainScenario.Eval();
	}
}
