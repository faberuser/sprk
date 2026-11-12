using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class NewbieRewardAttendanceData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public EAttendanceKeyword Keyword { get; set; }

	[Key(2)]
	public NewbieRewardType TabType { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "Index=" + Index;
	}
}
