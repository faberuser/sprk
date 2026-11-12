namespace NShared;

[JsonSerializable]
public class CommonConfig_RedisHost
{
	public string type { get; set; }

	public string ip { get; set; }

	public int port { get; set; }

	public string pass { get; set; }
}
