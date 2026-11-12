namespace NShared;

[JsonSerializable]
public class CommonConfig_Redis
{
	public CommonConfig_RedisHost[] player { get; set; }

	public CommonConfig_RedisHost[] message { get; set; }
}
