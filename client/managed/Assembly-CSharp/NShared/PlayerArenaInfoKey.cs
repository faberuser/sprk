namespace NShared;

public class PlayerArenaInfoKey : Tuple<MatchArenaType, int>
{
	public PlayerArenaInfoKey(MatchArenaType arenaType, int seasonIndex)
		: base(arenaType, seasonIndex)
	{
	}
}
