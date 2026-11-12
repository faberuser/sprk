namespace NShared;

[JsonSerializable(Javascript = true)]
public class Replay
{
	public long Uid { get; set; }

	public string Info { get; set; }

	public string Data { get; set; }
}
