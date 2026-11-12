using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CreatureLevelData : IDataKeySelector<int>
{
	int IDataKeySelector<int>.Key => Level;

	[Key(0)]
	public int Level { get; set; }

	[Key(1)]
	public long LocalExp { get; set; }

	[Key(2)]
	public long TotalExp { get; set; }

	public override string ToString()
	{
		return "Level=" + Level;
	}
}
