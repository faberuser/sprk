using System;

namespace NShared.B2M_Ping;

[JsonSerializable(Javascript = true)]
public class QueuedCheckResult
{
	public IntegrityInfo CheckInfo { get; set; }

	public string Result { get; set; }

	public string ExactResult { get; set; }

	public long ClientTotalDamage { get; set; }

	public DateTime QueuedTime { get; set; }
}
