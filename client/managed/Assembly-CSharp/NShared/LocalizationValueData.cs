using MessagePack;
using NShared.NLocalization;

namespace NShared;

[MessagePackObject(false)]
public class LocalizationValueData : IDataKeySelector<LanguageType>
{
	[Key(0)]
	public LanguageType Language { get; set; }

	[Key(1)]
	public bool Open { get; set; }

	[Key(2)]
	public bool Voice { get; set; }

	[Key(3)]
	public string CsEmail { get; set; }

	[Key(4)]
	public string AppName { get; set; }

	[Key(5)]
	public string PluginLanguageName { get; set; }

	[Key(6)]
	public int CountryFlagIndex { get; set; }

	[Key(7)]
	public string CultureInfoName { get; set; }

	[IgnoreMember]
	public LanguageType Key => Language;

	LanguageType IDataKeySelector<LanguageType>.Key => Language;

	public override string ToString()
	{
		return "LocalizationValueData LanguageType=" + Language;
	}
}
