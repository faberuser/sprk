namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class GuildBuildingLevelUp
{
	public int GuildBuildingIndex { get; set; }

	public byte GuildBuildingLevel { get; set; }

	public string UpdatedTime { get; set; }

	public int Wood { get; set; }

	public int Stone { get; set; }

	public int Metal { get; set; }
}
