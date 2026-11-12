namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class GuildSuppressKilled
{
	public long[] KillAccountIds { get; set; }

	public int Level { get; set; }
}
