using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class InfinityData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int StartValue { get; set; }

	[Key(2)]
	public int[] IncreaseValues { get; set; }

	[Key(3)]
	public int Multiplier { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
