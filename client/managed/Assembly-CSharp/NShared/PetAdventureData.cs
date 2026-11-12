using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PetAdventureData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public EPetAdventureType AdventureType { get; set; }

	[Key(2)]
	public int MaxPetCount { get; set; }

	[Key(3)]
	public bool IsShow { get; set; }

	[Key(4)]
	public int Order { get; set; }

	[Key(5)]
	public int ReqFood { get; set; }

	[Key(6)]
	public int MaxDeckCount { get; set; }

	[Key(7)]
	public string NameKey { get; set; }

	[Key(8)]
	public string DescKey { get; set; }

	[Key(9)]
	public string ImagePath { get; set; }

	[Key(10)]
	public string ThumbnailImagePath { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
