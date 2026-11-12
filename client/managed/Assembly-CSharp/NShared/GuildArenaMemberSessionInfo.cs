namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildArenaMemberSessionInfo
{
	public int SessionIndex { get; set; }

	public string ServerGroup { get; set; }

	public long GuildId { get; set; }

	public long AccountId { get; set; }

	public int SessionScore { get; set; }

	public int MatchWinCount { get; set; }

	public int MatchLoseCount { get; set; }

	public int DefenceWinCount { get; set; }

	public int DefenceLoseCount { get; set; }

	public int DeckCount { get; set; }

	public int LoseDeck1 { get; set; }

	public int LoseDeck2 { get; set; }

	public int LoseDeck3 { get; set; }

	public int LoseDeck4 { get; set; }

	public int LoseDeck5 { get; set; }

	public int BlindDeck1 { get; set; }

	public int BlindDeck2 { get; set; }

	public int BlindDeck3 { get; set; }

	public int BlindDeck4 { get; set; }

	public int BlindDeck5 { get; set; }
}
