namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class WorldBossKilled
{
	public int WorldBossIndex { get; set; }

	public long KillAccountId { get; set; }

	public string KillUserName { get; set; }
}
