namespace NShared;

public class CampaignDungeonKey : Tuple<int, int>
{
	public int ChapterIndex => Item1;

	public int DungeonIndex => Item2;

	public CampaignDungeonKey()
		: base(0, 0)
	{
	}

	public CampaignDungeonKey(int chapterIndex, int dungeonIndex)
		: base(chapterIndex, dungeonIndex)
	{
	}
}
