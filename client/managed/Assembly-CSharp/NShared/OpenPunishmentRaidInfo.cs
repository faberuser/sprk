namespace NShared;

[JsonSerializable(Javascript = true)]
public class OpenPunishmentRaidInfo : IAccountAttrKey<OpenPunishmentRaidKey>
{
	OpenPunishmentRaidKey IAccountAttrKey<OpenPunishmentRaidKey>.Key => new OpenPunishmentRaidKey(GroupIndex, DungeonType);

	public int GroupIndex { get; set; }

	public int DungeonType { get; set; }

	public int IsOpen { get; set; }

	public string OpenedTime { get; set; }

	public string ExpireTime { get; set; }

	public int OpenLevel { get; set; }

	public int ClearLevel { get; set; }

	public int ClearCount { get; set; }
}
