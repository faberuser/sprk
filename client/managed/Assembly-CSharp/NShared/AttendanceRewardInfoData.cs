using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class AttendanceRewardInfoData : IDataKeySelector<AttendanceRewardInfoKey>
{
	AttendanceRewardInfoKey IDataKeySelector<AttendanceRewardInfoKey>.Key => new AttendanceRewardInfoKey(Index, Day, No);

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int Day { get; set; }

	[Key(2)]
	public int No { get; set; }

	[Key(3)]
	public string RewardTitleKey { get; set; }

	[Key(4)]
	public string RewardDescKey { get; set; }

	public override string ToString()
	{
		return $"Index : {Index} Day : {Day} No : {No}";
	}
}
