namespace NShared;

[JsonSerializable]
public class GuildAttendanceInfo
{
	public long GuildId { get; set; }

	public int Daily { get; set; }

	public int Weekly { get; set; }

	public string UpdatedTime { get; set; }

	public int LastDaily { get; set; }

	public string LastDailyUpdatedTime { get; set; }

	public int LastWeekly { get; set; }

	public string LastWeeklyUpdatedTime { get; set; }
}
