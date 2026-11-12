using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct AttendanceUrlDataKey
{
	[Key(0)]
	public int Index;

	[Key(1)]
	public string Language;

	public AttendanceUrlDataKey(int field_0, string field_1)
	{
		Index = field_0;
		Language = field_1;
	}

	public bool Equals(AttendanceUrlDataKey other)
	{
		if (Index == other.Index)
		{
			return Language == other.Language;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Index, Language);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({Index}, {Language})";
	}
}
