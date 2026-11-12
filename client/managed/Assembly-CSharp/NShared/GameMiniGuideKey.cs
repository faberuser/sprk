namespace NShared;

public class GameMiniGuideKey : Tuple<int, int>
{
	public int ChapterIndex => Item1;

	public int DungeonIndex => Item2;

	public GameMiniGuideKey()
		: base(0, 0)
	{
	}

	public GameMiniGuideKey(int chapterIndex, int dungeonIndex)
		: base(chapterIndex, dungeonIndex)
	{
	}
}
