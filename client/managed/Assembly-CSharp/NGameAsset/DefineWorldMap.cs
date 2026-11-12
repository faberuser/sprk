namespace NGameAsset;

public static class DefineWorldMap
{
	public static class VersionHistory
	{
		public const string Version_20150717 = "20150717";
	}

	public const string BundleTypeName = "[WorldMap]";

	public const string VersionCode = "20150717";

	public static string ObjectName = "MainObject";

	public static string BundleCode(string code)
	{
		return "WorldMap." + (String_Extension_Method.IsNullOrEmpty(code) ? "(noname)" : code);
	}
}
