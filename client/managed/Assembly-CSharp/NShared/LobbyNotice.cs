namespace NShared;

[JsonSerializable(Javascript = true)]
public class LobbyNotice
{
	public int Index { get; set; }

	public string Name { get; set; }

	public string BeginTime { get; set; }

	public string EndTime { get; set; }

	public LobbyNoticeLocalInfo[] LocalInfo { get; set; }

	public LobbyNoticeTaskInfo[] TaskInfo { get; set; }
}
