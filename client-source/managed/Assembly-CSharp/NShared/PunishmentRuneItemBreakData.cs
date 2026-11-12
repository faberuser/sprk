using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class PunishmentRuneItemBreakData : IDataKeySelector<int>
{
	[Key(0)]
	public int BreakIndex { get; set; }

	[Key(1)]
	public string ItemCode1 { get; set; }

	[Key(2)]
	public int MinItemCount1 { get; set; }

	[Key(3)]
	public int MaxItemCount1 { get; set; }

	[Key(4)]
	public int MinItemCount2 { get; set; }

	[Key(5)]
	public int MaxItemCount2 { get; set; }

	[Key(6)]
	public int MinItemCount3 { get; set; }

	[Key(7)]
	public int MaxItemCount3 { get; set; }

	[Key(8)]
	public int MinItemCount4 { get; set; }

	[Key(9)]
	public int MaxItemCount4 { get; set; }

	[Key(10)]
	public int MinItemCount5 { get; set; }

	[Key(11)]
	public int MaxItemCount5 { get; set; }

	int IDataKeySelector<int>.Key => BreakIndex;
}
