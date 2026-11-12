using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class AttendanceRewardUrlData : IDataKeySelector<AttendanceRewardUrlKey>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int Day { get; set; }

	[Key(2)]
	public string Language { get; set; }

	[Key(3)]
	public string Url { get; set; }

	AttendanceRewardUrlKey IDataKeySelector<AttendanceRewardUrlKey>.Key => new AttendanceRewardUrlKey(Index, Day, Language);

	public override string ToString()
	{
		return $"Index : {Index}, Day : {Day},  Language : {Language}, Url : {Url}";
	}
}
