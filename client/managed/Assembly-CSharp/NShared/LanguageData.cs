using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class LanguageData : IDataKeySelector<string>
{
	[Key(0)]
	public string Language { get; set; }

	string IDataKeySelector<string>.Key => Language;
}
