using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PortalPropData : IDataKeySelector<int>
{
	[IgnoreMember]
	public int AutoIncrementedIndex;

	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int DungeonIndex { get; set; }

	[Key(2)]
	public int PositionX { get; set; }

	[Key(3)]
	public int PositionY { get; set; }

	[Key(4)]
	public PortalPropType PropType { get; set; }

	[Key(5)]
	public string PropName { get; set; }

	[Key(6)]
	public string[] PropAction { get; set; }

	[Key(7)]
	public int Scale { get; set; }

	[Key(8)]
	public int Angle { get; set; }

	[Key(9)]
	public PortalPropConditionType ConditionType { get; set; }

	[Key(10)]
	public string[] ConditionArgs { get; set; }

	int IDataKeySelector<int>.Key => AutoIncrementedIndex;
}
