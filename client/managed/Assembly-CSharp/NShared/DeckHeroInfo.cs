namespace NShared;

[JsonSerializable(Javascript = true)]
public class DeckHeroInfo
{
	public int Index { get; set; }

	public int Star { get; set; }

	public int Level { get; set; }
}
