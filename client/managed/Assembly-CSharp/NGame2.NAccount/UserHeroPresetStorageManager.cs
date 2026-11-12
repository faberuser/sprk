using NShared;

namespace NGame2.NAccount;

public class UserHeroPresetStorageManager : NShared.UserHeroPresetStorageManager
{
	public enum EChagnedType
	{
		None,
		Add,
		Remove,
		Override,
		ChangeName
	}

	public class HeroPresetStorageChangedInfo
	{
		public EChagnedType ChangedType;

		public string StorageKey;
	}

	private readonly UserObserverManager<HeroPresetStorageChangedInfo> _observerManager = new UserObserverManager<HeroPresetStorageChangedInfo>();

	public IUserObserverManager ObserverManager => _observerManager;

	public void Apply(HeroPresetStorageInfo heroPresetStorageInfo)
	{
		if (heroPresetStorageInfo != null)
		{
			EChagnedType chagnedType = EChagnedType.Add;
			string storageKey = heroPresetStorageInfo.StorageKey;
			if (ContainKey(storageKey))
			{
				Remove(storageKey);
				chagnedType = EChagnedType.Override;
			}
			Add(heroPresetStorageInfo);
			AnnounceToObserver(chagnedType, storageKey);
		}
	}

	public void ApplyName(string storageKey, string name)
	{
		HeroPresetStorageInfo heroPresetStorageInfo = Get(storageKey);
		if (heroPresetStorageInfo != null)
		{
			heroPresetStorageInfo.Name = name;
			AnnounceToObserver(EChagnedType.ChangeName, storageKey);
		}
	}

	public override bool Remove(HeroPresetStorageInfo value)
	{
		if (value == null)
		{
			return false;
		}
		return Remove(value.StorageKey);
	}

	public override bool Remove(string key)
	{
		if (base.Remove(key))
		{
			AnnounceToObserver(EChagnedType.Remove, key);
			return true;
		}
		return false;
	}

	public void AddObserver(IUserObserver<HeroPresetStorageChangedInfo> observer)
	{
		_observerManager.AddObserver(observer);
	}

	public void RemoveObserver(IUserObserver<HeroPresetStorageChangedInfo> observer)
	{
		_observerManager.RemoveObserver(observer);
	}

	private void AnnounceToObserver(EChagnedType chagnedType, string storageKey)
	{
		HeroPresetStorageChangedInfo info = new HeroPresetStorageChangedInfo
		{
			ChangedType = chagnedType
		};
		AnnounceToObserver(info);
	}

	private void AnnounceToObserver(HeroPresetStorageChangedInfo info)
	{
		if (info != null)
		{
			_observerManager.AnnounceToObservers(info);
		}
	}
}
