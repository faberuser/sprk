namespace NShared;

[JsonSerializable(Javascript = true)]
public class AttendanceRewardLocalInfo
{
	public int Index { get; set; }

	public int Day { get; set; }

	public string Language { get; set; }

	public string Description { get; set; }

	public string ImageUrl { get; set; }

	public string DetailTextKey { get; set; }
}
