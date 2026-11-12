using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

[Serializable]
public class UseOnetimeBoosterPanel
{
	public UILabel LabelName;

	public UILabel LabelDesc;

	public UILabel LabelGem;

	public Transform OnetimeBoosterRoot;

	public Transform IconRoot;

	public UISprite SpriteCheckedOnetimeBootyBooster;

	public UISprite SpriteUnusableOnetimeBootyBooster;

	private ItemIconComponent.Listener _listener;

	private ItemIconComponent _itemIconComponent;

	private ItemData _itemData;

	private bool _isEnableOnetimeBoosterRoot;

	private bool _isUsableOnetimeBoosterItem;

	private bool _useOnetimeBooster;

	public bool EnableOnetimeBoosterRoot
	{
		get
		{
			return _isEnableOnetimeBoosterRoot;
		}
		set
		{
			_isEnableOnetimeBoosterRoot = value;
			SetRoot(_isEnableOnetimeBoosterRoot);
			if (!value)
			{
				UseOnetimeBoosterItem = false;
			}
		}
	}

	public bool UsableOnetimeBoosterItem
	{
		get
		{
			return _isUsableOnetimeBoosterItem;
		}
		set
		{
			_isUsableOnetimeBoosterItem = value;
			SetUnusableOnetimeBoosterItem(_isUsableOnetimeBoosterItem);
			if (!_isUsableOnetimeBoosterItem)
			{
				UseOnetimeBoosterItem = false;
			}
		}
	}

	public bool UseOnetimeBoosterItem
	{
		get
		{
			return _useOnetimeBooster;
		}
		set
		{
			_useOnetimeBooster = value;
			SetCheckedOnetimeBootyBooster(_useOnetimeBooster);
		}
	}

	private BoosterItemData GetBoosterItemData
	{
		get
		{
			IList<BoosterItemData> allData = GameRoot.Instance.BoosterItemDataContainer.GetAllData();
			if (__Extension_Method_ICollection__.IsNullOrEmpty(allData))
			{
				return null;
			}
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allData); i++)
			{
				BoosterItemData boosterItemData = allData[i];
				if (boosterItemData != null && boosterItemData.IsOnetime)
				{
					return boosterItemData;
				}
			}
			return null;
		}
	}

	public void Init(CampaignDungeonData dungeonData, bool isUseOnetimeBooster, bool isScenario, ItemIconComponent.Listener listener)
	{
		EnableOnetimeBoosterRoot = IsEnableOnetimeBoosterRoot(dungeonData);
		if (EnableOnetimeBoosterRoot)
		{
			UsableOnetimeBoosterItem = UIHelper.IsUsableOneTimeBoosterItem(dungeonData, isUseOnetimeBooster, isScenario);
			SetComponents(listener);
			if (IsForcedUseBoosterItem())
			{
				UseOnetimeBoosterItem = true;
			}
		}
	}

	public void CloseRoot()
	{
		SetRoot(isEnableRoot: false);
	}

	public void OnToggleUseOnetimeBoosterItem()
	{
		if (!UsableOnetimeBoosterItem || _itemData == null)
		{
			return;
		}
		long totalGem = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem;
		int buyGem = _itemData.BuyGem;
		if (!UseOnetimeBoosterItem)
		{
			int num = buyGem;
			if (totalGem < num)
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("BOOSTER_ONETIME_REWARD_INSUFFICIENT_RUBY"));
				return;
			}
		}
		UseOnetimeBoosterItem = !UseOnetimeBoosterItem;
	}

	public bool IsUseOnetimeBoosterItem()
	{
		if (EnableOnetimeBoosterRoot && UsableOnetimeBoosterItem)
		{
			return UseOnetimeBoosterItem;
		}
		return false;
	}

	public ItemData GetItemData()
	{
		return _itemData;
	}

	public long GetRepeatableCount(long curStaminaCount, int reqStaminaCount)
	{
		long num = GetRepeatableGemCount();
		if (num == 0L)
		{
			int num2 = 0;
			return num2;
		}
		if (reqStaminaCount > 0)
		{
			long val = curStaminaCount / reqStaminaCount;
			num = Math.Min(num, val);
		}
		return num;
	}

	private long GetRepeatableGemCount()
	{
		int num = ((_itemData != null) ? _itemData.BuyGem : 0);
		long totalGem = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem;
		if (num == 0)
		{
			int num2 = 0;
			return num2;
		}
		int num3 = num;
		return totalGem / num3;
	}

	private bool IsEnableOnetimeBoosterRoot(CampaignDungeonData dungeonData)
	{
		if (GetBoosterItemData == null)
		{
			return false;
		}
		SetOnetimeBossterItemData();
		if (_itemData == null)
		{
			return false;
		}
		if (dungeonData != null && dungeonData.BattleType == BattleType.WorldBoss)
		{
			return false;
		}
		return true;
	}

	private bool IsForcedUseBoosterItem()
	{
		if (AutoNextBattle.IsEnabled())
		{
			long itemBoosterCount = AutoNextBattle.instance.ItemBoosterCount;
			int num = 0;
			if (itemBoosterCount > num)
			{
				return true;
			}
		}
		return false;
	}

	private void SetComponents(ItemIconComponent.Listener listener)
	{
		BoosterItemData getBoosterItemData = GetBoosterItemData;
		if (getBoosterItemData != null && _itemData != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect(getBoosterItemData.OnetimeBoosterTitle);
			string stringDirect2 = LocalizationManager.GetStringDirect(getBoosterItemData.OnetimeBoosterDesc);
			UIHelper.SetLabelText(LabelName, stringDirect);
			UIHelper.SetLabelText(LabelDesc, stringDirect2);
			UIHelper.SetLabelText(LabelGem, _itemData.BuyGem * -1);
			if (!(IconRoot == null))
			{
				_itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(IconRoot);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_itemIconComponent.transform);
				_itemIconComponent.Open(_itemData, listener);
			}
		}
	}

	private void SetRoot(bool isEnableRoot)
	{
		UIHelper.EnableGameObject(OnetimeBoosterRoot, isEnableRoot);
	}

	private void SetCheckedOnetimeBootyBooster(bool useOnetimeBoosterItem)
	{
		UIHelper.EnableGameObject(SpriteCheckedOnetimeBootyBooster, useOnetimeBoosterItem);
	}

	private void SetUnusableOnetimeBoosterItem(bool usableOnetime)
	{
		UIHelper.EnableGameObject(SpriteUnusableOnetimeBootyBooster, !usableOnetime);
	}

	private void SetOnetimeBossterItemData()
	{
		BoosterItemData getBoosterItemData = GetBoosterItemData;
		if (getBoosterItemData != null)
		{
			int boosterItemIndex = getBoosterItemData.BoosterItemIndex;
			_itemData = GameRoot.Instance.ItemDataContainer.GetData(boosterItemIndex);
		}
	}

	public void Release()
	{
		ReleaseItemComponent();
		CloseRoot();
		_isEnableOnetimeBoosterRoot = false;
		UsableOnetimeBoosterItem = false;
		_itemData = null;
	}

	public void ReleaseItemComponent()
	{
		if (_itemIconComponent != null)
		{
			FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
		}
		_itemIconComponent = null;
	}
}
