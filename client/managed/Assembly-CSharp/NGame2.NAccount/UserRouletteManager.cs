using System.Collections.Generic;
using NShared;

namespace NGame2.NAccount;

public class UserRouletteManager : NShared.UserRouletteManager
{
	private readonly UserObserverManager<RouletteInfo> _observerManager = new UserObserverManager<RouletteInfo>();

	public IUserObserverManager ObserverManager => _observerManager;

	public void Apply(RouletteInfo info)
	{
		if (info != null)
		{
			Add(info);
			_observerManager.AnnounceToObservers(info);
		}
	}

	public void Apply(IList<RouletteInfo> info)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(info); i++)
		{
			Apply(info[i]);
		}
	}

	protected override void OnAddData(int key, RouletteInfo value)
	{
		base.OnAddData(key, value);
		_observerManager.AnnounceToObservers(value);
	}
}
