using NShared.NJit;

namespace NShared;

[JitKey]
public struct AchievementKey2
{
	public int Index;

	public int Step;

	public AchievementKey2(int index, int step)
	{
		Index = index;
		Step = step;
	}

	public bool Equals(AchievementKey2 other)
	{
		if (Index == other.Index)
		{
			return Step == other.Step;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Index, Step);
	}

	public override string ToString()
	{
		return $"Index={Index} Step={Step}";
	}
}
