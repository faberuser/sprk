namespace NShared;

public class RaidMultiInfoDataKey : Tuple<int, int, int>
{
	public int RaidIndex => Item1;

	public int Level => Item2;

	public int GroupIndex => Item3;

	public RaidMultiInfoDataKey()
		: base(0, 0, 0)
	{
	}

	public RaidMultiInfoDataKey(int raidIndex, int level, int groupIndex)
		: base(raidIndex, level, groupIndex)
	{
	}
}
