using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CensoredWordData : IDataKeySelector<string>
{
	string IDataKeySelector<string>.Key => Word;

	[Key(0)]
	public string Word { get; set; }

	[Key(1)]
	public bool AllowChat { get; set; }

	[Key(2)]
	public bool MatchWholeWord { get; set; }

	[Key(3)]
	public bool SimilarCheck { get; set; }

	[Key(4)]
	public bool SensitiveWord { get; set; }

	public override string ToString()
	{
		return Word;
	}
}
