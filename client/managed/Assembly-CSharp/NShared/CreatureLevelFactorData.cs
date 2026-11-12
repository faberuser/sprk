using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CreatureLevelFactorData : IDataKeySelector<CreatureLevelFactorDataKey>
{
	public enum FactorType
	{
		Hero = 1,
		Monster
	}

	CreatureLevelFactorDataKey IDataKeySelector<CreatureLevelFactorDataKey>.Key => new CreatureLevelFactorDataKey(FactorIndex, Level);

	[Key(0)]
	public int FactorIndex { get; set; }

	[Key(1)]
	public short Level { get; set; }

	[Key(2)]
	public long Value1 { get; set; }

	[Key(3)]
	public long Value2 { get; set; }

	public long GetValue(int valueIndex)
	{
		return valueIndex switch
		{
			1 => Value1, 
			2 => Value2, 
			_ => 0L, 
		};
	}

	public override string ToString()
	{
		return "FactorIndex=" + FactorIndex + " Level=" + Level;
	}
}
