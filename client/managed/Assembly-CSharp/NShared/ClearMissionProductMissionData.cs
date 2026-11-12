using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ClearMissionProductMissionData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int ProductGroupIndex { get; set; }

	[Key(2)]
	public ClearMissionProvideType ProvideType { get; set; }

	[Key(3)]
	public QuestActionType StateType { get; set; }

	[Key(4)]
	public int[] StateValue { get; set; }

	[Key(5)]
	public string MissionDesc { get; set; }

	[Key(6)]
	public string[] MissionDescStateValue { get; set; }

	[Key(7)]
	public long ReqProgress { get; set; }

	[Key(8)]
	public int RewardIndex { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "Index=" + Index;
	}
}
