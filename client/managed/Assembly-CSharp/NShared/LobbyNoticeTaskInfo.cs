namespace NShared;

[JsonSerializable(Javascript = true)]
public class LobbyNoticeTaskInfo
{
	public int Position { get; set; }

	public string TaskName { get; set; }

	public string TaskValue { get; set; }
}
