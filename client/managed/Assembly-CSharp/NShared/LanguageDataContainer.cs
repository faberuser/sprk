using NShared.NJit;

namespace NShared;

public class LanguageDataContainer : JitDataContainerDefault<string, LanguageData>
{
	public const string TABLENAME = "Server/Localization_Language";

	public override TablePathDefine PathDefine => new TablePathDefine("Server/Localization_Language");
}
