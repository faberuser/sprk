using NShared;

public static class __Extension_Method_NShared_ArenaReplayInfo__
{
	public static MatchPvpType GetPvpType(this ArenaReplayInfo This)
	{
		if (This != null && This.ArenaType == MatchArenaType.BanPick)
		{
			return MatchPvpType.BanPick;
		}
		return MatchPvpType.Normal;
	}
}
