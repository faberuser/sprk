using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class WorldMapBuildData
{
	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int BuildingIndex { get; set; }

	[Key(2)]
	public int PositionX { get; set; }

	[Key(3)]
	public int PositionY { get; set; }

	[Key(4)]
	public int Scale { get; set; }

	[Key(5)]
	public int Angle { get; set; }

	[Key(6)]
	public string ModelName { get; set; }

	[Key(7)]
	public string SelectorName { get; set; }

	[Key(8)]
	public string BuildingType { get; set; }

	public override string ToString()
	{
		return $"ChapterIndex {ChapterIndex} BuildingType : {BuildingType}";
	}
}
