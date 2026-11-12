using JsonBin;

namespace NGameAsset.NWorldMap;

public class MapImageData : ILoadJson
{
	public string BundleFileName = string.Empty;

	public string AssetName = string.Empty;

	public int DisplayWidth;

	public int DisplayHeight;

	bool ILoadJson.Load(JsonRead json)
	{
		BundleFileName = json["BundleFileName"].GETSTRING(BundleFileName);
		AssetName = json["AssetName"].GETSTRING(AssetName);
		DisplayWidth = json["DisplayWidth"].GETINT(DisplayWidth);
		DisplayHeight = json["DisplayHeight"].GETINT(DisplayHeight);
		return true;
	}
}
