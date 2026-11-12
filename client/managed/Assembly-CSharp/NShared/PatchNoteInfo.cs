namespace NShared;

[JsonSerializable(Javascript = true)]
public class PatchNoteInfo
{
	public int Index { get; set; }

	public string Title { get; set; }

	public string Message { get; set; }

	public string EndTime { get; set; }
}
