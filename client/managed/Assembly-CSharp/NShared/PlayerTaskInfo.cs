namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerTaskInfo : IAccountAttrKey<PlayerTaskType>
{
	PlayerTaskType IAccountAttrKey<PlayerTaskType>.Key => TaskKey;

	public PlayerTaskType TaskKey { get; set; }

	public string HandlerName { get; set; }

	public PlayerTaskState State { get; set; }

	public string BeginTime { get; set; }

	public string EndTime { get; set; }

	public string UpdatedTime { get; set; }

	public int FailCount { get; set; }

	public string ExtraInfo { get; set; }
}
