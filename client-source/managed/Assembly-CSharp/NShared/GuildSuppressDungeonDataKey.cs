namespace NShared;

public class GuildSuppressDungeonDataKey : Tuple<int, int>
{
	public int RaidIndex => Item1;

	public int Level => Item2;

	public GuildSuppressDungeonDataKey()
		: base(0, 0)
	{
	}

	public GuildSuppressDungeonDataKey(int raidIndex, int level)
		: base(raidIndex, level)
	{
	}
}
