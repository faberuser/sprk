namespace NShared;

[JsonSerializable(Javascript = true)]
public class AttendanceLocalInfo
{
	public int Index { get; set; }

	public string Language { get; set; }

	public string Title { get; set; }

	public string BackgroundImageUrl { get; set; }

	public string DescTextKey { get; set; }
}
