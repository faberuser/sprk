using NShared;

namespace NGame2.NAccount;

public class UserRightAwayProductManager : NShared.UserRightAwayProductManager
{
	public class ChangedRightAwayProductInfo
	{
		public RightAwayProductInfo[] RightAwayProductInfos;
	}

	private readonly UserObserverManager<ChangedRightAwayProductInfo> _observerManager = new UserObserverManager<ChangedRightAwayProductInfo>();

	public IUserObserverManager ObserverManager => _observerManager;

	private void AnnounceToObserver(RightAwayProductInfo[] rightAwayProductInfos)
	{
		if (_observerManager != null)
		{
			_observerManager.AnnounceToObservers(new ChangedRightAwayProductInfo
			{
				RightAwayProductInfos = rightAwayProductInfos
			});
		}
	}

	public override void Reset(RightAwayProductInfo[] values)
	{
		base.Reset(values);
		AnnounceToObserver(values);
	}

	public void Apply(RightAwayProductInfo[] infos, int[] removeIndices)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
		Remove(removeIndices);
		AnnounceToObserver(GetValues());
	}

	private void Apply(RightAwayProductInfo info)
	{
		Add(info);
	}

	public void Remove(int[] removeIndices)
	{
		if (removeIndices == null)
		{
			return;
		}
		foreach (int key in removeIndices)
		{
			if (ContainKey(key))
			{
				Remove(key);
			}
		}
	}

	public bool Any()
	{
		return Count() > 0;
	}
}
