using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount.NExtension;
using NShared;

namespace NGame2.NAccount;

public class UserItemTimeDurationManager : NShared.UserItemTimeDurationManager
{
	public enum BoosterChangeType
	{
		BoosterAdded,
		BoosterChanged
	}

	public class BoosterChangeInfo
	{
		public BoosterChangeType changeType;

		public List<PlayerItemTimeDurationInfo> ItemTimeDurationInfos = new List<PlayerItemTimeDurationInfo>();

		public void AddItemTimeDurationInfo(PlayerItemTimeDurationInfo info)
		{
			if (ItemTimeDurationInfos != null && info != null)
			{
				ItemTimeDurationInfos.Add(info);
			}
		}

		public void AddItemTimeDurationInfo(PlayerItemTimeDurationInfo[] infos)
		{
			if (ItemTimeDurationInfos != null && infos != null)
			{
				ItemTimeDurationInfos.AddRange(infos);
			}
		}
	}

	private readonly UserObserverManager<BoosterChangeInfo> _observerManager = new UserObserverManager<BoosterChangeInfo>();

	public IUserObserverManager ObserverManager => _observerManager;

	public void Apply(PlayerItemTimeDurationInfo Info)
	{
		if (Info != null)
		{
			PlayerItemTimeDurationInfo playerItemTimeDurationInfo = new PlayerItemTimeDurationInfo
			{
				ItemIndex = Info.ItemIndex,
				StartTime = Info.StartTime,
				EndTime = Info.EndTime
			};
			BoosterItemData boosterItamData = GetBoosterItamData(playerItemTimeDurationInfo.ItemIndex);
			if (boosterItamData != null)
			{
				playerItemTimeDurationInfo.ItemIndex = boosterItamData.BoosterItemIndex;
				Add(playerItemTimeDurationInfo);
				BoosterChangeInfo boosterChangeInfo = new BoosterChangeInfo();
				boosterChangeInfo.changeType = BoosterChangeType.BoosterAdded;
				boosterChangeInfo.AddItemTimeDurationInfo(playerItemTimeDurationInfo);
				_observerManager.AnnounceToObservers(boosterChangeInfo);
			}
		}
	}

	public void Apply(PlayerItemTimeDurationInfo[] infos)
	{
		if (infos == null)
		{
			return;
		}
		foreach (PlayerItemTimeDurationInfo playerItemTimeDurationInfo in infos)
		{
			BoosterItemData boosterItamData = GetBoosterItamData(playerItemTimeDurationInfo.ItemIndex);
			if (boosterItamData != null)
			{
				playerItemTimeDurationInfo.ItemIndex = boosterItamData.BoosterItemIndex;
			}
		}
		Set(infos);
		BoosterChangeInfo boosterChangeInfo = new BoosterChangeInfo();
		boosterChangeInfo.changeType = BoosterChangeType.BoosterChanged;
		boosterChangeInfo.AddItemTimeDurationInfo(infos);
		_observerManager.AnnounceToObservers(boosterChangeInfo);
	}

	private BoosterItemData GetBoosterItamData(int itemIndex)
	{
		BoosterItemData boosterItemData = null;
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
		if (data == null)
		{
			return GameRoot.Instance.BoosterItemDataContainer.GetBoosterDataByGroupIndex(itemIndex);
		}
		return GameRoot.Instance.BoosterItemDataContainer.GetData(data.Code);
	}

	public PlayerItemTimeDurationInfo GetEffectiveBooster(EventBonusType boosterType, BattleType battleType = BattleType.None)
	{
		using (IEnumerator<PlayerItemTimeDurationInfo> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				PlayerItemTimeDurationInfo current = enumerator.Current;
				if (IsEffectiveBooster(current, boosterType, battleType))
				{
					return current;
				}
			}
		}
		return null;
	}

	public int GetEffectiveBoosterAddPercent(EventBonusType boosterType, BattleType battleType = BattleType.None)
	{
		int num = 0;
		using IEnumerator<PlayerItemTimeDurationInfo> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerItemTimeDurationInfo current = enumerator.Current;
			if (IsEffectiveBooster(current, boosterType, battleType))
			{
				BoosterItemData boosterItamData = GetBoosterItamData(current.ItemIndex);
				if (boosterItamData != null)
				{
					num += boosterItamData.Value;
				}
			}
		}
		return num;
	}

	public bool IsEffectiveBooster(PlayerItemTimeDurationInfo info, EventBonusType boosterType, BattleType battleType = BattleType.None)
	{
		if (info == null)
		{
			return false;
		}
		_ = info.ItemIndex;
		_ = info.StartTime;
		BoosterItemData boosterItamData = GetBoosterItamData(info.ItemIndex);
		if (boosterItamData == null)
		{
			return false;
		}
		if (boosterItamData.Type != boosterType)
		{
			return false;
		}
		if (battleType != BattleType.None && __Extension_Method_ICollection__.COUNT(boosterItamData.BattleTypes) > 0 && !boosterItamData.BattleTypes.Contains(battleType))
		{
			return false;
		}
		if ((int)info.GetRemainTime().TotalSeconds <= 0)
		{
			return false;
		}
		return true;
	}

	public PlayerItemTimeDurationInfo GetByItemIndex(int itemIndex)
	{
		BoosterItemData boosterItamData = GetBoosterItamData(itemIndex);
		if (boosterItamData == null)
		{
			return null;
		}
		return Get(boosterItamData.BoosterItemIndex);
	}
}
