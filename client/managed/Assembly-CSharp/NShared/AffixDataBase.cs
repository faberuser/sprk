using System.Collections.Generic;
using System.Linq;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class AffixDataBase
{
	private HashSet<int> _hashHeroindex;

	private HashSet<long> _addedUid;

	public int Index { get; set; }

	public TeamType TeamType { get; set; }

	public int[] HeroIndices { get; set; }

	public bool CheckActiveCondition(CampaignDungeonData dungeonData)
	{
		if (dungeonData == null)
		{
			return false;
		}
		return true;
	}

	public bool IsAddAble(TeamType teamType, int heroIndex)
	{
		if (CheckTeam(teamType))
		{
			return CheckHeroIndices(heroIndex);
		}
		return false;
	}

	private bool CheckTeam(TeamType teamType)
	{
		if (TeamType == TeamType.None || TeamType == TeamType.All)
		{
			return true;
		}
		return TeamType == teamType;
	}

	private bool CheckHeroIndices(int heroIndex)
	{
		if (HeroIndices == null || HeroIndices.Length == 0)
		{
			return true;
		}
		if (_hashHeroindex == null)
		{
			_hashHeroindex = new HashSet<int>(HeroIndices);
		}
		return _hashHeroindex.Contains(heroIndex);
	}

	public void AddUid(long uid)
	{
		if (_addedUid == null)
		{
			_addedUid = new HashSet<long>();
		}
		if (!_addedUid.Contains(uid))
		{
			_addedUid.Add(uid);
		}
	}

	public long[] GetApplyUids()
	{
		return _addedUid.ToArray();
	}
}
