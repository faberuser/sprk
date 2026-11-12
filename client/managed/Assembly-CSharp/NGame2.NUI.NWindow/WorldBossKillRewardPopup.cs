using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class WorldBossKillRewardPopup : BaseWindow
{
	public interface Listener
	{
		void OnClickViewList(WorldBossKillRewardPopup caller);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/WorldBoss/WorldBossKillRewardPopup";

	public UIScrollGrid Grid_RewardItem;

	public UILabel Name;

	public UILabel Desc;

	private List<UIScrollItemBehaviour> _itemIconComponentList = new List<UIScrollItemBehaviour>();

	private Listener _listener;

	public void Open(Listener listener, PolledRewardInfo rewardInfo, string worldBossName)
	{
		base.Open();
		_listener = listener;
		ClearItemIconList();
		UIHelper.SetLabelText(Name, worldBossName);
		string text = LocalizationManager.GetString(WorldBoss.WORLDBOSS_KILL_REWARD_DESC, worldBossName);
		UIHelper.SetLabelText(Desc, text);
		if (rewardInfo.Gem > 0)
		{
			CurrencyIconComponent currencyIconComponent = UIHelper.AddGridItem(Grid_RewardItem, CurrencyIconComponent.PrefabAuto);
			int num = Convert.ToInt32(rewardInfo.Gem);
			CurrencyIconComponent.Listener listener2 = null;
			int num2 = num;
			currencyIconComponent.Open(CurrencyType.Gem, num2, listener2);
			_itemIconComponentList.Add(currencyIconComponent);
		}
		if (rewardInfo.Gold > 0)
		{
			CurrencyIconComponent currencyIconComponent2 = UIHelper.AddGridItem(Grid_RewardItem, CurrencyIconComponent.PrefabAuto);
			int num3 = Convert.ToInt32(rewardInfo.Gem);
			CurrencyIconComponent.Listener listener3 = null;
			int num4 = num3;
			currencyIconComponent2.Open(CurrencyType.Gold, num4, listener3);
			_itemIconComponentList.Add(currencyIconComponent2);
		}
		RewardItemInfo[] items = rewardInfo.Items;
		foreach (RewardItemInfo rewardItemInfo in items)
		{
			if (rewardItemInfo != null)
			{
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardItemInfo.Code);
				if (dataByCode != null)
				{
					ItemIconComponent itemIconComponent = UIHelper.AddGridItem(Grid_RewardItem, ItemIconComponent.PrefabAuto);
					int count = rewardItemInfo.Count;
					long count2 = count;
					itemIconComponent.Open(dataByCode);
					itemIconComponent.SetItemCount(count2);
					_itemIconComponentList.Add(itemIconComponent);
				}
			}
		}
		UIHelper.RepositionGrid(Grid_RewardItem);
	}

	public override void Reset()
	{
		ClearItemIconList();
	}

	private void ClearItemIconList()
	{
		FactoryBase.ReleaseAndClearList(_itemIconComponentList);
	}

	public void OnClickViewList()
	{
		if (_listener != null)
		{
			_listener.OnClickViewList(this);
		}
	}
}
