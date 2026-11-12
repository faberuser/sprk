using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PortalData : IDataKeySelector<ChapterDungeonKey2>
{
	ChapterDungeonKey2 IDataKeySelector<ChapterDungeonKey2>.Key => new ChapterDungeonKey2(ChapterIndex, DungeonIndex);

	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int DungeonIndex { get; set; }

	[Key(2)]
	public string Name { get; set; }

	[Key(3)]
	public int[] ParentNode { get; set; }

	[Key(4)]
	public int PositionX { get; set; }

	[Key(5)]
	public int PositionY { get; set; }

	[Key(6)]
	public string ImagePath { get; set; }

	[Key(7)]
	public string PopupImagePath { get; set; }

	[Key(8)]
	public bool HasChild { get; set; }

	[Key(9)]
	public bool ExecuteNodeFunction { get; set; }

	[Key(10)]
	public bool Hidden { get; set; }

	[Key(11)]
	public bool ShortCutButton { get; set; }

	[Key(12)]
	public string[] Condition { get; set; }

	[Key(13)]
	public string TitleDescKey { get; set; }

	[Key(14)]
	public bool ContentsShortCutButton { get; set; }

	[Key(15)]
	public string AdventureNoticeCode { get; set; }

	[Key(16)]
	public int OpenEffectPortalIndex { get; set; }

	[Key(17)]
	public CurrencyType PortalGaugeCurrencyType { get; set; }

	[Key(18)]
	public string PortalGaugePath { get; set; }

	public bool hasParent()
	{
		if (ParentNode != null && ParentNode.Length != 0 && ParentNode.Length == 2)
		{
			return true;
		}
		return false;
	}
}
