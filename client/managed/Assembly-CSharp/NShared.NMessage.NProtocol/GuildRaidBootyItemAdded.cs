namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class GuildRaidBootyItemAdded
{
	public GuildRaidBootyEquipItemInfo[] EquipItemInfos { get; set; }

	public GuildRaidBootyItemInfo[] ItemInfos { get; set; }
}
