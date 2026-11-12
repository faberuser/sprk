using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class PetIncubatorSlotData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public CurrencyType BuyCurrencyType { get; set; }

	[Key(2)]
	public int BuyCurrencyValue { get; set; }

	[Key(3)]
	public int OpenNum { get; set; }

	[Key(4)]
	public float[] Position { get; set; }

	[Key(5)]
	public float[] Rotation { get; set; }

	[Key(6)]
	public int DefaultIndex { get; set; }

	[Key(7)]
	public int Mileage { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
