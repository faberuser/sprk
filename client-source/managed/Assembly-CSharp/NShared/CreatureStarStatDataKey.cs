using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct CreatureStarStatDataKey
{
	[Key(0)]
	public int Star;

	[Key(1)]
	public int Transcended;

	[Key(2)]
	public StatType StatType;

	public CreatureStarStatDataKey(int star, int transcended, StatType statType)
	{
		Star = star;
		Transcended = transcended;
		StatType = statType;
	}

	public bool Equals(CreatureStarStatDataKey other)
	{
		if (Star == other.Star && Transcended == other.Transcended)
		{
			return StatType == other.StatType;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Star, Transcended, StatType);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({Star}, {Transcended}, {StatType})";
	}
}
