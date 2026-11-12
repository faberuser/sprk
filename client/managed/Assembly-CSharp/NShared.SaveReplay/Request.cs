namespace NShared.SaveReplay;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	public virtual string ServiceName => "replay/save_replay";

	public ReplayType Type { get; set; }

	public string Info { get; set; }

	public string BattleLogs { get; set; }

	public long[] AccountIds { get; set; }
}
