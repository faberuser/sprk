using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct KingsPassExpKey
{
	[Key(0)]
	public int SeasonIndex;

	[Key(1)]
	public StaminaType Type;

	public KingsPassExpKey(int seasonIndex, StaminaType type)
	{
		SeasonIndex = seasonIndex;
		Type = type;
	}

	public bool Equals(KingsPassExpKey other)
	{
		if (SeasonIndex == other.SeasonIndex)
		{
			return Type == other.Type;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(SeasonIndex, Type);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({SeasonIndex}, {Type})";
	}
}
