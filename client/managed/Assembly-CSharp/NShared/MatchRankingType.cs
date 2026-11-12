namespace NShared;

[JsonSerializable(Javascript = true)]
public enum MatchRankingType
{
	None,
	Server,
	World,
	ServerTotal
}
