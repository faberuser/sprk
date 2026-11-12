using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class DefenceLevelData : IDataKeySelector<int>
{
	[Key(0)]
	public int Level { get; set; }

	[Key(1)]
	public int MaxDefence { get; set; }

	int IDataKeySelector<int>.Key => Level;
}
