namespace NShared;

public class OpenPunishmentRaidKey : Tuple<int, int>
{
	public OpenPunishmentRaidKey(int groupIndex, int dungeonType)
		: base(groupIndex, dungeonType)
	{
	}
}
