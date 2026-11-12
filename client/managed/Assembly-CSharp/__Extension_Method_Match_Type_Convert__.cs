using NShared;

public static class __Extension_Method_Match_Type_Convert__
{
	public static bool TryToPvpType(this MatchArenaType This, out MatchPvpType pvpType)
	{
		switch (This)
		{
		case MatchArenaType.Normal:
			pvpType = MatchPvpType.Normal;
			return true;
		case MatchArenaType.BanPick:
			pvpType = MatchPvpType.BanPick;
			return true;
		default:
			pvpType = MatchPvpType.Normal;
			return false;
		}
	}

	public static MatchArenaType ToMatchArenaType(this MatchPvpType This)
	{
		if (This != MatchPvpType.Normal)
		{
			return MatchArenaType.BanPick;
		}
		return MatchArenaType.Normal;
	}

	public static ReplayType ToReplayType(this MatchArenaType This)
	{
		if (This != MatchArenaType.BanPick)
		{
			return ReplayType.Arena;
		}
		return ReplayType.BanPick;
	}

	public static ReplayType ToReplayType(this MatchPvpType This)
	{
		if (This != MatchPvpType.BanPick)
		{
			return ReplayType.Arena;
		}
		return ReplayType.BanPick;
	}
}
