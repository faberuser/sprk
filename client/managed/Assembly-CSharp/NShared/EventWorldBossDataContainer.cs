using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class EventWorldBossDataContainer : JitDataContainerDefault<EventWorldBossDataKey, EventWorldBossData>
{
	public const string TABLENAME = "EventWorldBossTable";

	private JitDictionary<int, HashSet<int>> _eventWorldBossIndices = new JitDictionary<int, HashSet<int>>();

	public override TablePathDefine PathDefine => new TablePathDefine("EventWorldBossTable");

	public EventWorldBossDataContainer()
	{
		base.Packer.RegisterAndBind(_eventWorldBossIndices);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (EventWorldBossData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_eventWorldBossIndices.JsonLoad_AddToHashSetValue(item.Index, item.BossLevel);
		}
	}

	public EventWorldBossData GetData(int index, int level)
	{
		return GetData(new EventWorldBossDataKey(index, level));
	}

	public ICollection<int> GetEventWorldBossIndices(int index)
	{
		return _eventWorldBossIndices.GetOrDefault(index);
	}
}
