namespace NShared;

public class TeamController : SnapshotObject
{
	private readonly SnapshotObjectMember<BattleCreature> _ownerHolder;

	private readonly SnapshotObjectMember<Team> _teamHolder;

	private readonly SnapshotObjectMember<bool> _swappedHolder;

	private readonly SnapshotObjectMember<SnapshotList<ITeamInverter>> _teamInverterListHolder;

	public Team Team => GetTeam();

	public int TeamId => GetTeamId();

	public TeamType TeamType => Team.Type;

	public SnapshotList<ITeamInverter> InverterTeamList => _teamInverterListHolder.Get();

	public TeamController()
	{
		_ownerHolder = new SnapshotObjectMember<BattleCreature>(this);
		_teamHolder = new SnapshotObjectMember<Team>(this);
		_swappedHolder = new SnapshotObjectMember<bool>(this, value: false);
		_teamInverterListHolder = new SnapshotObjectMember<SnapshotList<ITeamInverter>>(this);
	}

	public void Init(BattleCreature owner)
	{
		_ownerHolder.Set(owner);
		_teamInverterListHolder.Alloc();
	}

	public void SetTeam(Team team)
	{
		_teamHolder.Set(team);
	}

	public bool IsTeamInverted()
	{
		if (_teamInverterListHolder.Get().Count > 0)
		{
			return true;
		}
		return false;
	}

	public void AddInverter(ITeamInverter inverter)
	{
		if (!InverterTeamList.Contains(inverter))
		{
			InverterTeamList.Add(inverter);
		}
	}

	public bool RemoveInveter(ITeamInverter inverter)
	{
		return InverterTeamList.Remove(inverter);
	}

	public void Swap(bool swapped)
	{
		_swappedHolder.Set(swapped);
	}

	public Team GetInitialTeam()
	{
		return _teamHolder.Get();
	}

	public Team GetTeam()
	{
		if (!_swappedHolder.Get())
		{
			return _teamHolder.Get();
		}
		return _teamHolder.Get().EnemyTeam;
	}

	public int GetTeamId()
	{
		return GetTeam()?.Id ?? 0;
	}

	public bool IsFriendly(TeamController otherTeam)
	{
		if (otherTeam != null)
		{
			return otherTeam.TeamId == TeamId;
		}
		return false;
	}

	public bool IsFriendly(TeamType otherTeamType)
	{
		return otherTeamType == TeamType;
	}
}
