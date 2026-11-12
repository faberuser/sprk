using NShared;

public static class __ExtensionMethod_NShared_MatchResultInfo__
{
	public static MatchPvpType GetPvpType(this MatchResultInfo This)
	{
		if (This.ArenaType != MatchArenaType.Normal)
		{
			return MatchPvpType.BanPick;
		}
		return MatchPvpType.Normal;
	}
}
