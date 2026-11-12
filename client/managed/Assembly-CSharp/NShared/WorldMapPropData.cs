using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class WorldMapPropData
{
	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int PositionX { get; set; }

	[Key(2)]
	public int PositionY { get; set; }

	[Key(3)]
	public WorldMapPropType PropType { get; set; }

	[Key(4)]
	public string PropName { get; set; }

	[Key(5)]
	public string[] PropAction { get; set; }

	[Key(6)]
	public int Scale { get; set; }

	[Key(7)]
	public int Angle { get; set; }

	[Key(8)]
	public WorldMapPropConditionType ConditionType { get; set; }

	[Key(9)]
	public string[] ConditionArgs { get; set; }

	[Key(10)]
	public string LayerName { get; set; }
}
