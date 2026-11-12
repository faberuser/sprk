namespace NShared;

[JsonSerializable(Javascript = true)]
public class EclipseDungeonData
{
	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }
}
