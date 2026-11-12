using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class ContentsNoticePopupData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public ContentsNoticeType ContentsType { get; set; }

	[Key(2)]
	public int ChapterIndex { get; set; }

	[Key(3)]
	public int DungeonIndex { get; set; }

	[Key(4)]
	public ContentsNoticePopupType PopupType { get; set; }

	[Key(5)]
	public string ContentsImagePath { get; set; }

	[Key(6)]
	public string TitleDescKey { get; set; }

	[Key(7)]
	public bool UseRank { get; set; }

	[Key(8)]
	public bool UseHonoraryChamber { get; set; }

	[Key(9)]
	public string[] OpenCondition { get; set; }

	[Key(10)]
	public int TutorialIndex { get; set; }

	[Key(11)]
	public string[] OpenAction { get; set; }

	[Key(12)]
	public ContentsStatusType ContentsStatusType { get; set; }

	[Key(13)]
	public ContentsStatusSubType ContentsStatusSubType { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
