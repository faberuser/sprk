namespace NShared;

[JsonSerializable]
public class PatchAssetRootData
{
	public string Path { get; set; }

	public string Name { get; set; }

	public bool CheckIntegrity { get; set; }

	public string[] ExportSubPath { get; set; }

	public string[] ExportFiles { get; set; }

	public string[] ExcludeFiles { get; set; }

	public bool Recurse { get; set; }

	public EncryptType EncryptType { get; set; }

	public CookType CookType { get; set; }

	public PatchArchiveType ArchiveType { get; set; }
}
