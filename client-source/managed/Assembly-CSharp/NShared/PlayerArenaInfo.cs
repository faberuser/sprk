namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerArenaInfo : IAccountAttrKey<PlayerArenaInfoKey>
{
	PlayerArenaInfoKey IAccountAttrKey<PlayerArenaInfoKey>.Key => new PlayerArenaInfoKey(ArenaType, SeasonIndex);

	public MatchArenaType ArenaType { get; set; }

	public int SeasonIndex { get; set; }

	public int BestTier { get; set; }

	public int CurrentTier { get; set; }

	public string CachedAccountInfo { get; set; }

	public string UpdatedTime { get; set; }
}
