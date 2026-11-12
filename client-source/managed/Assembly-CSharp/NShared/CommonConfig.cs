namespace NShared;

[JsonSerializable]
public class CommonConfig
{
	public string data_path { get; set; }

	public string match_server_ip { get; set; }

	public int match_server_port { get; set; }

	public CommonConfig_Redis redis { get; set; }

	public CommonConfig_Server[] server { get; set; }

	public string public_ip_command { get; set; }

	public string[] public_ip_command_args { get; set; }

	public string private_ip_command { get; set; }

	public string[] private_ip_command_args { get; set; }
}
