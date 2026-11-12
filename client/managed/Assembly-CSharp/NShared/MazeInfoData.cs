using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class MazeInfoData : IDataKeySelector<int>
{
	[Key(0)]
	public int TowerIndex { get; set; }

	[Key(1)]
	public string TowerNameKey { get; set; }

	[Key(2)]
	public string InfoTitleKey { get; set; }

	[Key(3)]
	public string ReqClassDescKey { get; set; }

	[Key(4)]
	public string BattleInfoTitleKey { get; set; }

	[Key(5)]
	public string BattleInfoDescKey { get; set; }

	[Key(6)]
	public string ReqClass { get; set; }

	[Key(7)]
	public CreatureTagType ClassType { get; set; }

	int IDataKeySelector<int>.Key => TowerIndex;

	public override string ToString()
	{
		return "MazeInfoData Index=" + TowerIndex;
	}
}
