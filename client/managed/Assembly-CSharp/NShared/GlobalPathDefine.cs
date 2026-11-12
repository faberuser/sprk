using System.IO;
using NShared.NJit;

namespace NShared;

public static class GlobalPathDefine
{
	public static string Unity_Assets = "";

	public static string Unity_Assets_StreamingAssets = "";

	public static string TableData = "";

	public static string TableJit = "";

	public static string TableData_ContentsDefine = "";

	public static string LocalizationRepository_in_Editor = "";

	public static string Localization_TableJit = "";

	public static string Localization_BasicJit = "";

	public static void Init_for_Server(string dataPath)
	{
		string text = Path.Combine(dataPath, "TableData");
		string path = Path.Combine(dataPath, "TableJit");
		TableData = Path.GetFullPath(text);
		TableData_ContentsDefine = Path.GetFullPath(Path.Combine(text, "ContentsDefine"));
		TableJit = Path.GetFullPath(path);
		TableRoot.Instance.Reset();
	}
}
