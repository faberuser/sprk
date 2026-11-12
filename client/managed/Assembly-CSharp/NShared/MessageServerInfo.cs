namespace NShared;

[JsonSerializable]
public class MessageServerInfo
{
	public string Address { get; set; }

	public int Port { get; set; }
}
