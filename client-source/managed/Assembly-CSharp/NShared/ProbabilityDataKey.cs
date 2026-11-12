namespace NShared;

public class ProbabilityDataKey : Tuple<int, int>
{
	public int Index => Item1;

	public int SubIndex => Item2;

	public ProbabilityDataKey()
		: base(0, 0)
	{
	}

	public ProbabilityDataKey(int index, int subIndex)
		: base(index, subIndex)
	{
	}
}
