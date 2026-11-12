namespace NShared;

[JsonSerializable(Javascript = true)]
public class EventStepGroupServerInfo
{
	public int Step { get; set; }

	public int TargetScore { get; set; }
}
