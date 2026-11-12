using JsonBin;

namespace NGameAsset;

public class BundleFileInfo : ILoadJson
{
	public string Name = string.Empty;

	public bool ContainsSpec;

	public bool SplitLanguage;

	bool ILoadJson.Load(JsonRead json)
	{
		Name = json["Name"].GETSTRING(Name);
		ContainsSpec = json["ContainsSpec"].GETBOOLEAN(ContainsSpec);
		SplitLanguage = json["SplitLanguage"].GETBOOLEAN(SplitLanguage);
		return true;
	}
}
