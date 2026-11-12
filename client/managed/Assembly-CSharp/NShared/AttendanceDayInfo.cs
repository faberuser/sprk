namespace NShared;

[JsonSerializable(Javascript = true)]
public class AttendanceDayInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => AttendanceIndex;

	public int AttendanceIndex { get; set; }

	public int Day { get; set; }
}
