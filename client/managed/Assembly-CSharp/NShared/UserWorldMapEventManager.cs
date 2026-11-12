namespace NShared;

public class UserWorldMapEventManager : AccountAttrManager<WorldMapEventInfoKey, WorldMapEventInfo>
{
	public bool Remove(int eventIndex, int chapterIndex, int dungeonIndex)
	{
		return base.Remove(new WorldMapEventInfoKey(eventIndex, chapterIndex, dungeonIndex));
	}

	public WorldMapEventInfo Get(int eventIndex, int chapterIndex, int dungeonIndex)
	{
		return Get(new WorldMapEventInfoKey(eventIndex, chapterIndex, dungeonIndex));
	}
}
