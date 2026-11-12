namespace NShared;

[JsonSerializable(Javascript = true)]
public class Notice
{
	public int Id { get; set; }

	public string BeginTime { get; set; }

	public string EndTime { get; set; }

	public string Message { get; set; }

	public int Oneshot { get; set; }

	public int Interval { get; set; }

	public NoticeLocalInfo[] LocalInfo { get; set; }
}
