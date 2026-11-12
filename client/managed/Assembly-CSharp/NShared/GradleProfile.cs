namespace NShared;

[JsonSerializable]
public class GradleProfile
{
	public string Name { get; set; }

	public string AppNameConfig { get; set; }

	public string ApplicaionId { get; set; }

	public string GoogleAppId { get; set; }

	public string IconPath { get; set; }

	public string Tasks { get; set; }

	public string ReleaseTasks { get; set; }

	public string[] ExcludeJars { get; set; }

	public string[] ExcludeModules { get; set; }

	public string[] ExcludeArchitectures { get; set; }

	public bool AppendSecretKey { get; set; }
}
