namespace NShared;

[JsonSerializable]
public class VersionedHost
{
	public string Type { get; set; }

	public string Description { get; set; }

	public AppVersionInfo[] AppVersion { get; set; }

	public AppServerInfo[] Server { get; set; }
}
