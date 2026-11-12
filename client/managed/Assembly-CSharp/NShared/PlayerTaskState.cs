namespace NShared;

[JsonSerializable(Javascript = true)]
public enum PlayerTaskState
{
	None,
	Wait,
	Completed,
	Failed,
	Rollback
}
