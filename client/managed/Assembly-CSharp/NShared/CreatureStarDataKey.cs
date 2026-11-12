using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct CreatureStarDataKey
{
	[Key(0)]
	public int Star;

	[Key(1)]
	public int Transcended;

	public CreatureStarDataKey(int star, int transcended)
	{
		Star = star;
		Transcended = transcended;
	}

	public bool Equals(CreatureStarDataKey other)
	{
		if (Star == other.Star)
		{
			return Transcended == other.Transcended;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Star, Transcended);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({Star}, {Transcended})";
	}
}
