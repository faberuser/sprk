using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class SkillProjectileData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int SkillIndex { get; set; }

	[Key(2)]
	public int DurationMs { get; set; }

	[Key(3)]
	public int[] TickIntervalMs { get; set; }

	[Key(4)]
	public int RadiusMm { get; set; }

	[Key(5)]
	public int Width { get; set; }

	[Key(6)]
	public int SpeedMms { get; set; }

	[Key(7)]
	public bool FollowTarget { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "Index=" + Index;
	}
}
