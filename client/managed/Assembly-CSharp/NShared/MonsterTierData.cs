using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class MonsterTierData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public long PhysicalAttack { get; set; }

	[Key(2)]
	public long MagicalAttack { get; set; }

	[Key(3)]
	public long PhysicalDefense { get; set; }

	[Key(4)]
	public long MagicalDefense { get; set; }

	[Key(5)]
	public long MaxHp { get; set; }

	[Key(6)]
	public int MaxMp { get; set; }

	[Key(7)]
	public int AntiCcChance { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "Index=" + Index;
	}
}
