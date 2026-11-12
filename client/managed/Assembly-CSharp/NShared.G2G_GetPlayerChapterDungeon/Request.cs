namespace NShared.G2G_GetPlayerChapterDungeon;

[Request]
public class Request
{
	public long AccountId { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }
}
