using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class SimilarAlphabetData : IDataKeySelector<char>
{
	[Key(0)]
	public char Alphabet { get; set; }

	[Key(1)]
	public char[] SimilarAlphabet { get; set; }

	char IDataKeySelector<char>.Key => Alphabet;

	public override string ToString()
	{
		return Alphabet.ToString();
	}
}
