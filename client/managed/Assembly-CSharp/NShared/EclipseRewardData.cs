using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EclipseRewardData : IDataKeySelector<int>
{
	int IDataKeySelector<int>.Key => Wave;

	[Key(0)]
	public int Wave { get; set; }

	[Key(1)]
	public int NormalReward { get; set; }

	[Key(2)]
	public int BestTeamReward { get; set; }
}
