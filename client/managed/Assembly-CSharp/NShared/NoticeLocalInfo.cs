namespace NShared;

[JsonSerializable(Javascript = true)]
public class NoticeLocalInfo
{
	public string Language { get; set; }

	public string Message { get; set; }
}
