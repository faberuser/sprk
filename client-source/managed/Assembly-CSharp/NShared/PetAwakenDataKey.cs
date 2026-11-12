using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct PetAwakenDataKey
{
	[Key(0)]
	public int PetIndex;

	[Key(1)]
	public int Star;

	public PetAwakenDataKey(int petIndex, int star)
	{
		PetIndex = petIndex;
		Star = star;
	}

	public bool Equals(PetAwakenDataKey other)
	{
		if (PetIndex == other.PetIndex)
		{
			return Star == other.Star;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(PetIndex, Star);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({PetIndex}, {Star})";
	}
}
