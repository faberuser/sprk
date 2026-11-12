using NShared;

namespace NVespa;

public class DownloadContextData
{
	public int CoroutineIndex;

	public int RemotePatchVersion;

	public ArchiveInfo ArchiveInfo;

	public ThreadSafeDic<string, AssetInfo> AssetInfoDictionary;
}
