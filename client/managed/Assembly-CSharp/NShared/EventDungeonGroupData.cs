using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class EventDungeonGroupData : IDataKeySelector<EventDungeonGroupKey>
{
	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int GroupIndex { get; set; }

	[Key(2)]
	public EDungeonGroupType GroupType { get; set; }

	[Key(3)]
	public string DungeonName { get; set; }

	[Key(4)]
	public string DungeonDesc { get; set; }

	[Key(5)]
	public string PortraitPath { get; set; }

	[Key(6)]
	public int[] DungeonIndices { get; set; }

	[Key(7)]
	public string BackGroundPath { get; set; }

	EventDungeonGroupKey IDataKeySelector<EventDungeonGroupKey>.Key => new EventDungeonGroupKey(ChapterIndex, GroupIndex);

	public override string ToString()
	{
		return "EventDungeonGroupData ChapterIndex=" + ChapterIndex + ", GroupIndex=" + GroupIndex;
	}
}
