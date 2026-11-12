namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class GuildRaidOpen
{
	public string Nick { get; set; }

	public GuildRaidDungeonInfo OpenedGuildRaidInfo { get; set; }
}
