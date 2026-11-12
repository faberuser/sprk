namespace NShared;

[JsonSerializable(Javascript = true)]
public class AttendanceInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => AttendanceIndex;

	public int AttendanceIndex { get; set; }

	public int CompletedDay { get; set; }

	public string CompletedTime { get; set; }

	public string EndTime { get; set; }

	public int LastRewardedDay { get; set; }

	public string SuspendEndTime { get; set; }
}
