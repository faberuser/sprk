using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EquipTransitionTicketData : IDataKeySelector<int>
{
	int IDataKeySelector<int>.Key => ItemIndex;

	[Key(0)]
	public int ItemIndex { get; set; }

	[Key(1)]
	public EquipTransitionType Type { get; set; }

	[Key(2)]
	public int[] SelectAbleDetailIndex { get; set; }

	[Key(3)]
	public int LeastStar { get; set; }

	[Key(4)]
	public int[] FixedGiveItem { get; set; }

	[Key(5)]
	public int[] FixedGetItem { get; set; }

	[Key(6)]
	public int[] ExceptItemGroup1 { get; set; }
}
