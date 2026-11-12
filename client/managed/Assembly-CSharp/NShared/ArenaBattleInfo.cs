namespace NShared;

public interface ArenaBattleInfo
{
	int MatchScore { get; set; }

	int SeasonWin { get; set; }

	int SeasonLose { get; set; }

	int SuccessiveWin { get; set; }

	int SuccessiveLose { get; set; }

	int TierIndex { get; set; }

	int WorldTierIndex { get; set; }

	string LastMatchTime { get; set; }
}
