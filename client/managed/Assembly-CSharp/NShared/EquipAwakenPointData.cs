using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EquipAwakenPointData : IDataKeySelector<int>
{
	[Key(0)]
	public int Star { get; set; }

	[Key(1)]
	public int AwakenPoint { get; set; }

	int IDataKeySelector<int>.Key => Star;

	public override string ToString()
	{
		return "Star=" + Star + " AwakenPoint=" + AwakenPoint;
	}
}
