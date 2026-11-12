using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct BottomButtonKey
{
	[Key(0)]
	public ERightMenuButton Type;

	[Key(1)]
	public string SubType;

	public BottomButtonKey(ERightMenuButton type, string subType)
	{
		Type = type;
		SubType = subType;
	}

	public bool Equals(BottomButtonKey other)
	{
		if (Type == other.Type)
		{
			return SubType == other.SubType;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Type, SubType);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({Type}, {SubType})";
	}
}
