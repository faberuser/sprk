namespace NShared;

[JsonSerializable(Javascript = true)]
public class UserTimeInfo
{
	public string LastLoginTime { get; set; }

	public string LastLogoutTime { get; set; }

	public string InactiveTime { get; set; }
}
