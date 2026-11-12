using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct InspectUserKey
{
	[Key(0)]
	public InspectUserCategory Category;

	[Key(1)]
	public InspectUserType Type;

	public InspectUserKey(InspectUserCategory category, InspectUserType type)
	{
		Category = category;
		Type = type;
	}

	public bool Equals(InspectUserKey other)
	{
		if (Category == other.Category)
		{
			return Type == other.Type;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Category, Type);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({Category}, {Type})";
	}
}
