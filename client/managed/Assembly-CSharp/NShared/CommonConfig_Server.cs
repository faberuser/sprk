namespace NShared;

[JsonSerializable]
public class CommonConfig_Server
{
	public string name { get; set; }

	public string type { get; set; }

	public string group { get; set; }

	public string public_ip { get; set; }

	public string private_ip { get; set; }

	public int port { get; set; }

	public bool standalone { get; set; }

	public bool is_default { get; set; }

	public bool sensitive_debugging { get; set; }

	public int no { get; set; }
}
