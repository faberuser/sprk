using NShared.NJit;

namespace NShared;

[JitKey]
public struct CreatureStarGradeStatKey
{
	public int CreatureIndex;

	public int Star;

	public int Transcended;

	public CreatureStarGradeStatKey(int creatureIndex, int star, int transcended)
	{
		CreatureIndex = creatureIndex;
		Star = star;
		Transcended = transcended;
	}

	public bool Equals(CreatureStarGradeStatKey other)
	{
		if (CreatureIndex == other.CreatureIndex && Star == other.Star)
		{
			return Transcended == other.Transcended;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(CreatureIndex, Star, Transcended);
	}
}
