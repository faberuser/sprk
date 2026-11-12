namespace NShared;

public class PunishmentRaidKey : Tuple<int, int>
{
	public PunishmentRaidKey()
		: base(0, 0)
	{
	}

	public PunishmentRaidKey(int raidIndex, int raidLevel)
		: base(raidIndex, raidLevel)
	{
	}
}
