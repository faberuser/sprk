using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class PetIncubatorData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int Grade { get; set; }

	[Key(2)]
	public string NameLocalKey { get; set; }

	[Key(3)]
	public string DescLocalKey { get; set; }

	[Key(4)]
	public int TimePer { get; set; }

	[Key(5)]
	public CurrencyType BuyCurrencyType { get; set; }

	[Key(6)]
	public int BuyCurrencyValue { get; set; }

	[Key(7)]
	public int Mileage { get; set; }

	[Key(8)]
	public string IconPath { get; set; }

	[Key(9)]
	public string CreatureCode { get; set; }

	[Key(11)]
	public int[] CreaturePosition { get; set; }

	[Key(12)]
	public int[] CreatureRotation { get; set; }

	[Key(13)]
	public int CreatureScale { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
