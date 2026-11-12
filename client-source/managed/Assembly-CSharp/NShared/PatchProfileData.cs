namespace NShared;

[JsonSerializable]
public class PatchProfileData
{
	public string Profile { get; set; }

	public PatchAssetRootData[] AssetRoots { get; set; }
}
