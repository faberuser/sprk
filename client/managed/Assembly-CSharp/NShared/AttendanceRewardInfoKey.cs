using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct AttendanceRewardInfoKey
{
	[Key(0)]
	public int Index;

	[Key(1)]
	public int Day;

	[Key(2)]
	public int No;

	public AttendanceRewardInfoKey(int index, int day, int no)
	{
		Index = index;
		Day = day;
		No = no;
	}

	public bool Equals(AttendanceRewardInfoKey other)
	{
		if (Index == other.Index && Day == other.Day)
		{
			return No == other.No;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Index, Day, No);
	}

	public override string ToString()
	{
		return $"Index={Index} Day={Day} No={No}";
	}
}
