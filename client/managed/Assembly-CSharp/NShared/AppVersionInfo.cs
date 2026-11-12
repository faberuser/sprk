namespace NShared;

[JsonSerializable]
public class AppVersionInfo
{
	public string Platform { get; set; }

	public string MinVersion { get; set; }

	public string HotfixVersion { get; set; }

	public string PatchVersionUrl { get; set; }

	public string CdnProxyUrl { get; set; }

	public string CrashReport { get; set; }

	public string OffBackgroundDownload { get; set; }

	public string DownloadFileWriteSleepTimeMS { get; set; }

	public string IsETC1TextureCdn { get; set; }

	public string LegacyLoadingBundlePack { get; set; }

	public string LegacyLoadingBundlePack_x86 { get; set; }
}
