namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class PartyDeckHeroCancel
{
	public enum ResultType
	{
		DECK_CANCEL_NONE,
		DECK_CANCEL_FULL,
		DECK_CANCEL_CONTAIN
	}

	public ResultType Result { get; set; }

	public long MemberId { get; set; }

	public int HeroIndex { get; set; }

	public int DeckIndex { get; set; }
}
