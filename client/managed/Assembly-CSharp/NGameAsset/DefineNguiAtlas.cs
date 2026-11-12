namespace NGameAsset;

public static class DefineNguiAtlas
{
	public static class VersionHistory
	{
		public const string Version_20160311 = "20160311";
	}

	public const string BundleTypeName = "[NguiAtlas]";

	public const string VersionCode = "20160311";

	public static string ObjectName = "MainObject";

	public static string BundleCode(string code)
	{
		return "NguiAtlas." + (String_Extension_Method.IsNullOrEmpty(code) ? "(noname)" : code);
	}
}
