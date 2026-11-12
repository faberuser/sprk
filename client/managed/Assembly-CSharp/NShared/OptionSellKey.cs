using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct OptionSellKey
{
	[Key(0)]
	public OptionSellCategory Category;

	[Key(1)]
	public int Value;

	public OptionSellKey(OptionSellCategory category, int value)
	{
		Category = category;
		Value = value;
	}

	public bool Equals(OptionSellKey other)
	{
		if (Category == other.Category)
		{
			return Value == other.Value;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Category, Value);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({Category}, {Value})";
	}
}
