namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildArenaMemberDeckInfo : GuildMemberServerInfo
{
	public int DeckCount { get; set; }

	public int[] DefeatDeck { get; set; }
}
