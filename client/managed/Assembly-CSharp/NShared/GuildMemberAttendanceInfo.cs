namespace NShared;

[JsonSerializable]
public class GuildMemberAttendanceInfo
{
	public long GuildId { get; set; }

	public long AccountId { get; set; }

	public string LastAttendanceTime { get; set; }

	public int Day1 { get; set; }

	public int Day2 { get; set; }

	public int Day3 { get; set; }

	public int Day4 { get; set; }

	public int Day5 { get; set; }

	public int Day6 { get; set; }

	public int Day7 { get; set; }

	public int Daily { get; set; }

	public int Weekly { get; set; }

	public int Successive { get; set; }

	public int SuccessiveCount { get; set; }

	public int MaxSuccessiveCount { get; set; }

	public string LastWeekFirstAttendanceTime { get; set; }

	public string UpdatedTime { get; set; }
}
