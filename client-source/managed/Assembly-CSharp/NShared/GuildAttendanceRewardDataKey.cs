using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct GuildAttendanceRewardDataKey
{
	[Key(0)]
	public int Index;

	[Key(1)]
	public int Day;

	public GuildAttendanceRewardDataKey(int index, int day)
	{
		Index = index;
		Day = day;
	}

	public bool Equals(GuildAttendanceRewardDataKey other)
	{
		if (Index == other.Index)
		{
			return Day == other.Day;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Index, Day);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({Index}, {Day})";
	}
}
