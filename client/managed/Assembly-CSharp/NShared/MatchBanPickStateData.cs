using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class MatchBanPickStateData : IDataKeySelector<MatchBanPickState>
{
	[Key(0)]
	public MatchBanPickState State { get; set; }

	[Key(1)]
	public TeamType TeamType { get; set; }

	[Key(2)]
	public bool IsPick { get; set; }

	[Key(3)]
	public int SelectableCount4Deck { get; set; }

	[Key(4)]
	public int SelectableCount8Deck { get; set; }

	[Key(5)]
	public string LocalMessage1P { get; set; }

	[Key(6)]
	public string LocalMessage2P { get; set; }

	[Key(7)]
	public int WaitTime4Deck { get; set; }

	[Key(8)]
	public int PenaltyTime4Deck1 { get; set; }

	[Key(9)]
	public int PenaltyTime4Deck2 { get; set; }

	[Key(10)]
	public int PenaltyTime4Deck3 { get; set; }

	[Key(11)]
	public string[] AnimMe1P { get; set; }

	[Key(12)]
	public string[] AnimMe2P { get; set; }

	MatchBanPickState IDataKeySelector<MatchBanPickState>.Key => State;

	public override string ToString()
	{
		return "State=" + State;
	}
}
