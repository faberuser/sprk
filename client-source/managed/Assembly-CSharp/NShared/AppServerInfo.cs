namespace NShared;

[JsonSerializable]
public class AppServerInfo
{
	public string Name { get; set; }

	public string Host { get; set; }

	public bool Opened { get; set; }

	public bool Https { get; set; }

	public string CookieKeyword { get; set; }

	public string[] Country { get; set; }

	public string ShutdownMessage { get; set; }

	public string QueryServerStatusUrl { get; set; }

	public string LoginServer { get; set; }
}
