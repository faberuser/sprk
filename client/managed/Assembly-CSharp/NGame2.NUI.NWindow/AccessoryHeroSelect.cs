using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class AccessoryHeroSelect : BaseWindow, HeroPortraitComponent.Listener
{
	public interface Listener
	{
		void ShowPreview(int heroIndex, AccessoryCostumeData accessoryCostumeData);

		void OnConfirm(int itemIndex, int accessoryIndex, int heroIndex, bool isAlternativeReward, Action onComplete);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Inventory/AccessoryHeroSelect";

	public UILabel LabelName;

	public UILabel LabelMessage;

	public Transform ButtonRootReceiveReward;

	public UIButton ButtonReceiveReward;

	public Transform ButtonRootSelectHero;

	public UIButton ButtonPreview;

	public UIButton ButtonConfirm;

	public UIScrollGrid ListGrid;

	private Listener _listener;

	private List<Component> _rewardList = new List<Component>();

	private List<HeroPortraitComponent> _heroPortraitList = new List<HeroPortraitComponent>();

	private ItemData _itemData;

	private AccessorySelectItemData _accessorySelectItemData;

	private AccessoryCostumeData _accessoryCostumeData;

	private HeroPortraitComponent _selectedHeroPortrait;

	private CachedHeroInfo _heroInfo;

	private bool _isAlternativeReward;

	private bool _disableConfime;

	public void Open(ItemData itemData, Listener listener, bool disableConfime = false)
	{
		if (listener == null)
		{
			VespaDebug.LogError("Listener == null");
			return;
		}
		base.Open();
		_itemData = itemData;
		_listener = listener;
		_disableConfime = disableConfime;
		_accessorySelectItemData = itemData.GetSubData<AccessorySelectItemData>();
		_accessoryCostumeData = GameRoot.Instance.AccessoryCostumeDataContainer.GetData(_accessorySelectItemData.AccessoryIndex);
		if (_accessoryCostumeData == null)
		{
			Close();
			return;
		}
		_isAlternativeReward = CheckAlternativeRewerd();
		SetItemName();
		SetMessageLabel();
		if (!_isAlternativeReward)
		{
			SetSelectHero();
		}
		else
		{
			SetAlternativeReward();
		}
		UpdateButtonState();
	}

	private bool CheckAlternativeRewerd()
	{
		int num = 0;
		List<CreatureData> heroListByOpenType = GameRoot.Instance.CreatureDataContainer.GetHeroListByOpenType(CreatureOpenType.Opened, CreatureOpenType.Scenario, CreatureOpenType.ChapterReward, CreatureOpenType.RecruitOnly);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroListByOpenType); i++)
		{
			int index = heroListByOpenType[i].Index;
			if (IsAccessoryAvailableHero(index) && !IsAccessoryOwnedHero(index))
			{
				num++;
			}
		}
		return num == 0;
	}

	private void SetSelectHero()
	{
		CreateHeroList(IsAccessoryAvailableHero);
	}

	private bool IsAccessoryAvailableHero(int heroIndex)
	{
		int[] targetHero = _accessorySelectItemData.TargetHero;
		int[] unableHero = _accessoryCostumeData.UnableHero;
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(unableHero) && TypeIterator.Contains(unableHero, heroIndex))
		{
			return false;
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(targetHero) && !TypeIterator.Contains(targetHero, heroIndex))
		{
			return false;
		}
		return true;
	}

	private void SetAlternativeReward()
	{
		if (_accessorySelectItemData.AllOwnedRewardIndex != 0)
		{
			int allOwnedRewardIndex = _accessorySelectItemData.AllOwnedRewardIndex;
			_rewardList = UIHelper.CreateRewardItemIconComponentList(allOwnedRewardIndex, ListGrid, includeCurrency: true);
			UIHelper.RepositionGrid(ListGrid);
		}
	}

	private void SetItemName()
	{
		UIHelper.SetLabelText(LabelName, _itemData.Name);
	}

	private void SetMessageLabel()
	{
		string stringDirect = LocalizationManager.GetStringDirect("CONTENT_CUSTOMIZE_ACCESSORY_EQUIP_HERO_POPUP_DESC");
		if (_isAlternativeReward)
		{
			stringDirect = LocalizationManager.GetStringDirect("CONTENT_CUSTOMIZE_ACCESSORY_EQUIP_HERO_HAVE_ALL_POPUP_DESC", _itemData.Name);
		}
		UIHelper.SetLabelText(LabelMessage, stringDirect);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_heroPortraitList);
		FactoryBase.ReleaseAndClearList(_rewardList);
		_heroInfo = null;
	}

	private void CreateHeroList(Func<int, bool> predicate)
	{
		List<CachedHeroInfo> list = new List<CachedHeroInfo>();
		List<CreatureData> heroListByOpenType = GameRoot.Instance.CreatureDataContainer.GetHeroListByOpenType(CreatureOpenType.Opened, CreatureOpenType.Scenario, CreatureOpenType.ChapterReward, CreatureOpenType.RecruitOnly);
		foreach (CachedHeroInfo heroInfo in GlobalAutoBehaviour<AccountManager>.instance.HeroManager)
		{
			if (!heroListByOpenType.Any((CreatureData e) => e.Index == heroInfo.HeroIndex))
			{
				CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroInfo.HeroIndex);
				if (data != null)
				{
					heroListByOpenType.Add(data);
				}
			}
		}
		heroListByOpenType.Sort((CreatureData l, CreatureData r) => l.Index.CompareTo(r.Index));
		foreach (CreatureData item in heroListByOpenType)
		{
			if (predicate == null || Vespa_System_Action_Extension_Method.fire(predicate, item.Index))
			{
				CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(item.Index);
				if (cachedHeroInfo == null)
				{
					cachedHeroInfo = new CachedHeroInfo(item);
				}
				list.Add(cachedHeroInfo);
			}
		}
		foreach (CachedHeroInfo item2 in list)
		{
			if (item2 != null)
			{
				HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(ListGrid, HeroPortraitComponent.SmallPrefabAuto);
				heroPortraitComponent.Open(item2, this, null, HeroPortraitComponent.EPortraitMode.Icon);
				heroPortraitComponent.SetDamageType(enable: false);
				heroPortraitComponent.SetShowClassTag(enable: false);
				heroPortraitComponent.ShowStar(show: false);
				heroPortraitComponent.SetLevel(show: false);
				if (GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(item2.HeroIndex) == null)
				{
					heroPortraitComponent.SetNotOwnedSmall();
				}
				if (IsAccessoryOwnedHero(item2.HeroIndex))
				{
					heroPortraitComponent.SetCustomLabelSmall(LocalizationManager.GetStringDirect("CONTENTS_CUSTOMIZE_ACCESSORY_HAVE_NOW"));
				}
				_heroPortraitList.Add(heroPortraitComponent);
			}
		}
		UIHelper.RepositionGrid(ListGrid);
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
		SelectHero(heroPortrait.HeroInfo);
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent heroPortrait, bool isMine)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo heroInfo)
	{
	}

	public void OnClickClose()
	{
		Close();
	}

	public void OnClickPreview()
	{
		_listener.ShowPreview(_heroInfo.HeroIndex, _accessoryCostumeData);
	}

	public void OnClickConfirm()
	{
		Action onComplete = delegate
		{
			Close();
		};
		_listener.OnConfirm(_itemData.Index, _accessoryCostumeData.Index, _heroInfo.HeroIndex, _isAlternativeReward, onComplete);
	}

	public void OnClickAlternativeReward()
	{
		Action onComplete = delegate
		{
			Close();
		};
		_listener.OnConfirm(_itemData.Index, _accessoryCostumeData.Index, 0, _isAlternativeReward, onComplete);
	}

	private void DeselectHero()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_heroPortraitList); i++)
		{
			_heroPortraitList[i].SetSelectSmall(select: false);
		}
	}

	private bool IsOwnHero(int heroIndex)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroIndex) == null)
		{
			return false;
		}
		return true;
	}

	private bool IsAccessoryOwnedHero(int heroIndex)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.AccessoryCostumeManager.Get(new PlayerAccessoryCostumeInfoKey(heroIndex, _accessorySelectItemData.AccessoryIndex)) == null)
		{
			return false;
		}
		return true;
	}

	private void SelectHero(CachedHeroInfo heroInfo)
	{
		if (heroInfo != null && IsOwnHero(heroInfo.HeroIndex) && !IsAccessoryOwnedHero(heroInfo.HeroIndex) && _heroInfo != heroInfo)
		{
			_heroInfo = heroInfo;
			DeselectHero();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_heroPortraitList); i++)
			{
				HeroPortraitComponent heroPortraitComponent = _heroPortraitList[i];
				bool selectSmall = Vespa_Common_Class_Extension_Method.EQUALS(heroPortraitComponent.HeroInfo, heroInfo);
				heroPortraitComponent.SetSelectSmall(selectSmall);
			}
			UpdateButtonState();
		}
	}

	private void UpdateButtonState()
	{
		UIHelper.EnableGameObject(ButtonRootReceiveReward, _isAlternativeReward);
		UIHelper.EnableGameObject(ButtonRootSelectHero, !_isAlternativeReward);
		UIHelper.EnableButton(ButtonReceiveReward, __Extension_Method_ICollection__.COUNT(_rewardList) > 0, grayscaleIfDisable: true);
		UIHelper.EnableButton(ButtonPreview, _heroInfo != null, grayscaleIfDisable: true);
		if (_disableConfime)
		{
			UIHelper.EnableButton(ButtonConfirm, enable: false);
		}
		else
		{
			UIHelper.EnableButton(ButtonConfirm, _heroInfo != null, grayscaleIfDisable: true);
		}
	}
}
