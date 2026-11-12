using NVespa;

namespace NGameAsset;

public static class DefineBattleFieldMap2
{
	public static class VersionHistory
	{
		public const string Version_20151207 = "20151207";
	}

	public const string BundleTypeName = "[BattleFieldMap2]";

	public const string VersionCode = "20151207";

	public static string ObjectName = "MainObject";

	public static LocalStorageBool NoAlphaMaterial = new LocalStorageBool(i_default: false, typeof(DefineBattleFieldMap2).FullName + "NoAlphaMaterial");

	public static string BundleCode(string code)
	{
		return "BattleFieldMap2." + (String_Extension_Method.IsNullOrEmpty(code) ? "(noname)" : code);
	}
}
