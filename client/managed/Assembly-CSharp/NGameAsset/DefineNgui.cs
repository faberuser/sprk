namespace NGameAsset;

public static class DefineNgui
{
	public static class VersionHistory
	{
		public const string Version_20150812 = "20150812";
	}

	public const string BundleTypeName = "[Ngui]";

	public const string VersionCode = "20150812";

	public static string ObjectName = "MainObject";

	public static string BundleCode(string code)
	{
		return "Ngui." + (String_Extension_Method.IsNullOrEmpty(code) ? "(noname)" : code);
	}
}
