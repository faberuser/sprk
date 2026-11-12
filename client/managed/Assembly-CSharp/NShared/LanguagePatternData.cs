using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class LanguagePatternData : IDataKeySelector<LanguagePatternKey>
{
	[Key(0)]
	public string ServerName { get; set; }

	[Key(1)]
	public int Index { get; set; }

	[Key(2)]
	public string PatternRange { get; set; }

	LanguagePatternKey IDataKeySelector<LanguagePatternKey>.Key => new LanguagePatternKey(ServerName, Index);
}
