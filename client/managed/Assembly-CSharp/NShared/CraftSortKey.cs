using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct CraftSortKey
{
	[Key(0)]
	public CraftSortCategory Category;

	[Key(1)]
	public int Value;

	public CraftSortKey(CraftSortCategory category, int value)
	{
		Category = category;
		Value = value;
	}

	public bool Equals(CraftSortKey other)
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
