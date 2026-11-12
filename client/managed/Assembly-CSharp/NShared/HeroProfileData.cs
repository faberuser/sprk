using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class HeroProfileData : IDataKeySelector<int>
{
	int IDataKeySelector<int>.Key => Index;

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string Gender { get; set; }

	[Key(2)]
	public string Race { get; set; }

	[Key(3)]
	public string Age { get; set; }

	[Key(4)]
	public int Length { get; set; }

	[Key(5)]
	public int[] Birth { get; set; }

	[Key(6)]
	public string MonthText { get; set; }

	[Key(7)]
	public string Constellation { get; set; }

	[Key(8)]
	public string Like { get; set; }

	[Key(9)]
	public string Unlike { get; set; }

	[Key(10)]
	public string Story { get; set; }

	[Key(11)]
	public string CVKor { get; set; }

	[Key(12)]
	public string CVJap { get; set; }

	[Key(13)]
	public string[] Emoti { get; set; }

	[Key(14)]
	public string HeroName { get; set; }

	public override string ToString()
	{
		return "Index=" + Index;
	}
}
