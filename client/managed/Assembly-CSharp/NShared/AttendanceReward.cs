namespace NShared;

[JsonSerializable(Javascript = true)]
public class AttendanceReward : DbRewardData
{
	public int Index { get; set; }

	public int Day { get; set; }

	public int No { get; set; }

	public bool Invisible { get; set; }

	public bool SendMail { get; set; }

	public EAttendanceRewardShowType ShowType { get; set; }
}
