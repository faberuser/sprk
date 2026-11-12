using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EventCraftData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string ItemCode { get; set; }

	[Key(2)]
	public string ShowGroup { get; set; }

	[Key(3)]
	public int RewardIndex { get; set; }

	[Key(4)]
	public int ShowIndex { get; set; }

	[Key(5)]
	public string MaterialItemCode1 { get; set; }

	[Key(6)]
	public int MaterialItemCount1 { get; set; }

	[Key(7)]
	public string MaterialItemCode2 { get; set; }

	[Key(8)]
	public int MaterialItemCount2 { get; set; }

	[Key(9)]
	public string MaterialItemCode3 { get; set; }

	[Key(10)]
	public int MaterialItemCount3 { get; set; }

	[Key(11)]
	public string MaterialItemCode4 { get; set; }

	[Key(12)]
	public int MaterialItemCount4 { get; set; }

	[Key(13)]
	public string MaterialItemCode5 { get; set; }

	[Key(14)]
	public int MaterialItemCount5 { get; set; }

	[Key(15)]
	public string MaterialItemCode6 { get; set; }

	[Key(16)]
	public int MaterialItemCount6 { get; set; }

	[Key(17)]
	public string MaterialItemCode7 { get; set; }

	[Key(18)]
	public int MaterialItemCount7 { get; set; }

	[Key(19)]
	public string MaterialItemCode8 { get; set; }

	[Key(20)]
	public int MaterialItemCount8 { get; set; }

	[Key(21)]
	public int CraftCount { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "EventCraftData Index=" + Index;
	}
}
