namespace NShared;

[JsonSerializable(Javascript = true)]
public class ContentsNoticePopupInfo
{
	public ContentsNoticeType ContentsType { get; set; }

	public int SeasonIndex { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public int SessionIndex { get; set; }
}
