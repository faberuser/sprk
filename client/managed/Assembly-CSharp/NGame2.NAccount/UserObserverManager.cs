using System.Collections.Generic;
using NVespa;

namespace NGame2.NAccount;

public class UserObserverManager<T> : IUserObserverManager
{
	private readonly List<IUserObserver> _observerList = new List<IUserObserver>();

	public UserObserverManager()
	{
		UserObserverManagerContainer.ManagerList.Add(new WeakRef<IUserObserverManager>(this));
	}

	public void AddObserver(IUserObserver observer)
	{
		if (observer is IUserObserver<T> && !_observerList.Contains(observer))
		{
			_observerList.Add(observer);
		}
	}

	public void RemoveObserver(IUserObserver observer)
	{
		_observerList.Remove(observer);
	}

	public void ClearObserver()
	{
		for (int num = __Extension_Method_ICollection__.COUNT(_observerList) - 1; num >= 0; num--)
		{
			if (!_observerList[num].IsGlobal())
			{
				_observerList.RemoveAt(num);
			}
		}
	}

	public void AnnounceToObservers(T changedInfo)
	{
		IUserObserver[] array = _observerList.ToArray();
		foreach (IUserObserver userObserver in array)
		{
			if (userObserver != null)
			{
				(userObserver as IUserObserver<T>).OnChanged(changedInfo);
			}
		}
	}
}
