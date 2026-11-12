using NShared;

namespace NGame2.NAccount;

public class UserItemUseManager : NShared.UserItemUseManager
{
	private readonly UserObserverManager<PlayerItemUseInfo> _observerManager;

	public IUserObserverManager ObserverManager => _observerManager;

	public UserItemUseManager()
	{
		_observerManager = new UserObserverManager<PlayerItemUseInfo>();
	}

	public void Apply(ItemUseResultInfo itemUseResultInfo)
	{
		if (itemUseResultInfo != null)
		{
			PlayerItemUseInfo playerItemUseInfo = Get(itemUseResultInfo.ItemIndex);
			if (playerItemUseInfo == null)
			{
				playerItemUseInfo = new PlayerItemUseInfo
				{
					ItemIndex = itemUseResultInfo.ItemIndex,
					OutItemIndex = itemUseResultInfo.OutItemIndex,
					UseCount = itemUseResultInfo.NewCount,
					CompletedTime = itemUseResultInfo.CompletedTime
				};
			}
			else
			{
				playerItemUseInfo.OutItemIndex = itemUseResultInfo.OutItemIndex;
				playerItemUseInfo.UseCount = itemUseResultInfo.NewCount;
				playerItemUseInfo.CompletedTime = itemUseResultInfo.CompletedTime;
			}
			Apply(playerItemUseInfo);
		}
	}

	public void Apply(ItemUseResultInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}

	public void Apply(PlayerItemUseInfo info)
	{
		Add(info);
		_observerManager.AnnounceToObservers(info);
	}

	public void Apply(PlayerItemUseInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}
}
