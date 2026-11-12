using MessagePack;

namespace NShared.NJit;

[MessagePackObject(false)]
[JitKey]
public struct Sample_JitKey_2
{
	[Key(0)]
	public int Field_0;

	[Key(1)]
	public int Field_1;

	public Sample_JitKey_2(int field_0, int field_1)
	{
		Field_0 = field_0;
		Field_1 = field_1;
	}

	public bool Equals(Sample_JitKey_2 other)
	{
		if (Field_0 == other.Field_0)
		{
			return Field_1 == other.Field_1;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Field_0, Field_1);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({Field_0}, {Field_1})";
	}
}
