using NShared.NJit;

namespace NShared;

[JitKey]
public struct AttendanceRewardUrlKey
{
	public int Index;

	public int Day;

	public string Language;

	public AttendanceRewardUrlKey(int index, int day, string language)
	{
		Index = index;
		Day = day;
		Language = language;
	}

	public bool Equals(AttendanceRewardUrlKey other)
	{
		if (Index == other.Index && Day == other.Day)
		{
			return Language == other.Language;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Index, Day, Language);
	}

	public override string ToString()
	{
		return string.Format("Index={0} Day={1} Language={1}", Index, Day, Language);
	}
}
