using MessagePack;
using NShared.NWorldMap;

namespace NShared;

[MessagePackObject(false)]
public class EventTriggerData : IDataKeySelector<int>, ITimerEvent
{
	int IDataKeySelector<int>.Key => Index;

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public EventTriggerCategory Category { get; set; }

	[Key(2)]
	public string EventType { get; set; }

	[Key(3)]
	public string[] EventCondition { get; set; }

	[Key(4)]
	public string[] CurrentCondition { get; set; }

	[Key(5)]
	public EventTriggerCommand Command { get; set; }

	[Key(6)]
	public string[] Argument { get; set; }

	[Key(7)]
	public string NpcCode { get; set; }

	[Key(8)]
	public string EventTitle { get; set; }

	[Key(9)]
	public string EventDescription { get; set; }

	[Key(10)]
	public bool ImmediatelyLoadComplate { get; set; }

	[Key(11)]
	public bool ForceLeaveParty { get; set; }

	public override string ToString()
	{
		return "[" + Index + "] = " + Command;
	}

	bool ITimerEvent.IsExpired()
	{
		return false;
	}
}
