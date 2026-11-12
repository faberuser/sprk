namespace NShared;

[JsonSerializable]
public class PatchGeneratorConfig
{
	public string ArchivePrefix { get; set; }

	public bool CreateArchive { get; set; }

	public int MinArchiveSize { get; set; }

	public PatchProfileData[] Profiles { get; set; }

	public string[] ExceptPattern { get; set; }

	public string[] ExceptHeroPattern { get; set; }

	public string[] IncludePattern { get; set; }

	public string[] AppstoreReviewExceptPattern { get; set; }
}
