namespace NShared;

public class SubStoryChapterDungeonKey : Tuple<int, int>
{
	public int ChapterIndex => Item1;

	public int DungeonIndex => Item2;

	public SubStoryChapterDungeonKey()
		: base(0, 0)
	{
	}

	public SubStoryChapterDungeonKey(int chapterIndex, int dungeonIndex)
		: base(chapterIndex, dungeonIndex)
	{
	}
}
