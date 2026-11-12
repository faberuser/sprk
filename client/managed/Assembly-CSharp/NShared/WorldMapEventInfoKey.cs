namespace NShared;

public class WorldMapEventInfoKey : Tuple<int, int, int>
{
	public WorldMapEventInfoKey(int eventIndex, int chapterIndex, int dungeonIndex)
		: base(eventIndex, chapterIndex, dungeonIndex)
	{
	}
}
