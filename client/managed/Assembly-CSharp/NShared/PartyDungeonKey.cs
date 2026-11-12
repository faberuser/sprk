namespace NShared;

public class PartyDungeonKey : Tuple<int, int>
{
	public int ChapterIndex => Item1;

	public int DungeonIndex => Item2;

	public PartyDungeonKey()
		: base(0, 0)
	{
	}

	public PartyDungeonKey(int chapterIndex, int dungeonIndex)
		: base(chapterIndex, dungeonIndex)
	{
	}
}
