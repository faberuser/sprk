using NShared.NJit;

namespace NShared;

[JitKey]
public struct ProbabilityDataKey2
{
	public int Index;

	public int SubIndex;

	public ProbabilityDataKey2(int index, int subIndex)
	{
		Index = index;
		SubIndex = subIndex;
	}

	public bool Equals(ProbabilityDataKey2 other)
	{
		if (Index == other.Index)
		{
			return SubIndex == other.SubIndex;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Index, SubIndex);
	}

	public override string ToString()
	{
		return $"Index={Index} SubIndex={SubIndex}";
	}
}
