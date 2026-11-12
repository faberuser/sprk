using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class GatewayToTrialData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int TowerIndex { get; set; }

	[Key(2)]
	public int Step { get; set; }

	[Key(3)]
	public int ChapterIndex { get; set; }

	[Key(4)]
	public int DungeonIndex { get; set; }

	[Key(5)]
	public int RewardIndex { get; set; }

	[Key(6)]
	public string DungeonName { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "Tower::GateWayToTrial index =" + Index;
	}
}
