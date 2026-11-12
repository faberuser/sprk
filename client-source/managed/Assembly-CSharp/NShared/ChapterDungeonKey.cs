namespace NShared;

public class ChapterDungeonKey : Tuple<int, int>
{
	public ChapterDungeonKey(int chapterIndex, int dungeonIndex)
		: base(chapterIndex, dungeonIndex)
	{
	}
}
