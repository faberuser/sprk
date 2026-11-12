using NShared;

namespace NGame2.NAccount;

public class UserTeamLevelBuffManager : NShared.UserTeamLevelBuffManager
{
	private readonly UserObserverManager<TeamLevelBuff> _observerManager = new UserObserverManager<TeamLevelBuff>();

	public IUserObserverManager ObserverManager => _observerManager;

	public override void Set(TeamLevelBuff[] values)
	{
		base.Set(values);
	}

	private void AnnounceToObservers(TeamLevelBuff info)
	{
		if (info != null)
		{
			_observerManager.AnnounceToObservers(info);
		}
	}

	public void Apply(TeamLevelBuff[] infos)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(infos))
		{
			foreach (TeamLevelBuff info in infos)
			{
				Apply(info);
			}
		}
	}

	public void Apply(TeamLevelBuff info)
	{
		if (info != null)
		{
			Add(info);
			AnnounceToObservers(info);
		}
	}
}
