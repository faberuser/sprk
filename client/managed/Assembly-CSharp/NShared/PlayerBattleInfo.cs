namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerBattleInfo : ArenaBattleInfo
{
	private int _tierIndex;

	public int MatchScore { get; set; }

	public int SeasonWin { get; set; }

	public int SeasonLose { get; set; }

	public int SuccessiveWin { get; set; }

	public int SuccessiveLose { get; set; }

	public int TierIndex
	{
		get
		{
			return _tierIndex;
		}
		set
		{
			SetTierIndex(value);
		}
	}

	public int WorldTierIndex
	{
		get
		{
			return _tierIndex;
		}
		set
		{
			SetTierIndex(value);
		}
	}

	public string LastMatchTime { get; set; }

	private void SetTierIndex(int tierIndex)
	{
		if (tierIndex > 0)
		{
			_tierIndex = tierIndex;
		}
	}
}
