using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct DbLogDataKey
{
	[Key(0)]
	public string Category;

	[Key(1)]
	public int Value;

	public DbLogDataKey(string category, int value)
	{
		Category = category;
		Value = value;
	}

	public bool Equals(DbLogDataKey other)
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
