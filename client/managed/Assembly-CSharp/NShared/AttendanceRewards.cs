namespace NShared;

[JsonSerializable(Javascript = true)]
public class AttendanceRewards
{
	public AttendanceReward[] Reward { get; set; }
}
