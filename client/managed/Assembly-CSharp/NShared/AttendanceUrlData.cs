using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class AttendanceUrlData : IDataKeySelector<AttendanceUrlDataKey>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string Language { get; set; }

	[Key(2)]
	public string Url { get; set; }

	AttendanceUrlDataKey IDataKeySelector<AttendanceUrlDataKey>.Key => new AttendanceUrlDataKey(Index, Language);

	public override string ToString()
	{
		return $"Index : {Index}, Language : {Language}, Url : {Url}";
	}
}
