using MessagePack;

namespace NShared.NJit;

[MessagePackObject(false)]
[JitKey]
public struct Sample_JitKey_4
{
	[Key(0)]
	public int Field_0;

	[Key(1)]
	public int Field_1;

	[Key(2)]
	public int Field_2;

	[Key(3)]
	public int Field_3;

	public Sample_JitKey_4(int field_0, int field_1, int field_2, int field_3)
	{
		Field_0 = field_0;
		Field_1 = field_1;
		Field_2 = field_2;
		Field_3 = field_3;
	}

	public bool Equals(Sample_JitKey_4 other)
	{
		if (Field_0 == other.Field_0 && Field_1 == other.Field_1 && Field_2 == other.Field_2)
		{
			return Field_3 == other.Field_3;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Field_0, Field_1, Field_2, Field_3);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({Field_0}, {Field_1}, {Field_2}, {Field_3})";
	}
}
