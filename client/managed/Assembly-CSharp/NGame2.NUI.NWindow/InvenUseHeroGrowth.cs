using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class InvenUseHeroGrowth : BaseWindow, HeroPortraitComponent.Listener
{
	public interface IListener
	{
		void OnConfirmHeroGrowth(int itemIndex, int heroIndex, bool allGrowth, int visualLevel, int visualStar, int visualTranscended, Action onComplete);

		void OpenHeroDetailView(int heroIndex, int visualLevel, int visualStar, int isualTranscended);
	}

	public const string PrefabPath = "Prefab2/Window/Inventory/InvenUseHeroGrowth";

	public Transform ComboboxRoot;

	public UIComboBox ComboBoxFilterMain;

	public UIComboBox ComboBoxFilterSub;

	public Transform SelectedHeroRoot;

	public UILabel LabelTitle;

	public UILabel LabelTouchHero;

	public UIButton ButtonConfirm;

	public UILabel LabelConfirm;

	public UIScrollGrid HeroListGrid;

	private IListener _iListener;

	private ItemData _itemData;

	private HeroGrowthItemData _heroGrowthItemData;

	private HeroPortraitComponent _selectedHeroPortrait;

	private List<HeroPortraitComponent> _heroPortraitList = new List<HeroPortraitComponent>();

	public void Open(IListener listener, ItemData itemData)
	{
		Open(delegate
		{
			Init(listener, itemData);
		}, null);
	}

	private void Init(IListener listener, ItemData itemData, CreatureData selectedHeroData = null)
	{
		_iListener = listener;
		if (itemData != null)
		{
			_itemData = itemData;
			_heroGrowthItemData = GameRoot.Instance.HeroGrowthItemDataContainer.GetData(itemData.Code);
			if (_heroGrowthItemData != null)
			{
				InitFilterComboBox();
				CreateHeroPortraitList(selectedHeroData);
				UIHelper.SetLabelText(LabelTitle, itemData.Name);
				string key = "CONFIRM_HERO_GROWTH_ITEM_BUTTON";
				UpdateInfoLabel(key);
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.SafeReleaseComponent(ref _selectedHeroPortrait);
		ClearHeroPortraitList();
		UpdateConfirmButtonState(isSelect: false);
	}

	private void CreateHeroPortraitList(CreatureData selectedHeroData = null)
	{
		RefreshHeroList();
		SelectHero(selectedHeroData, isScrollTo: true);
	}

	private void OnChangeHeroList(int val)
	{
		RefreshHeroList();
	}

	private void RefreshHeroList()
	{
		ClearHeroPortraitList();
		int heroLevel = _heroGrowthItemData.HeroLevel;
		int heroStar = _heroGrowthItemData.HeroStar;
		int heroTranscend = _heroGrowthItemData.HeroTranscend;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_heroGrowthItemData.HeroIndices); i++)
		{
			int heroIndex = _heroGrowthItemData.HeroIndices[i];
			AddHeroComponentByFilter(heroIndex, heroLevel, heroStar, heroTranscend);
		}
		UIHelper.RepositionGrid(HeroListGrid);
	}

	private void AddHeroComponentByFilter(int heroIndex, int visualLevel = 0, int visualStar = 0, int visualTranscend = 0)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		if (data == null)
		{
			return;
		}
		int currentKey = ComboBoxFilterMain.CurrentKey;
		CreatureTagType currentKey2 = (CreatureTagType)ComboBoxFilterSub.CurrentKey;
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroIndex);
		bool flag = cachedHeroInfo != null;
		bool num = cachedHeroInfo != null && cachedHeroInfo.Level >= visualLevel && cachedHeroInfo.Star >= visualStar && cachedHeroInfo.Transcended >= visualTranscend;
		bool selection = _selectedHeroPortrait != null && _selectedHeroPortrait.CreatureData != null && _selectedHeroPortrait.CreatureData.Index == data.Index;
		bool flag2 = num || !flag;
		if ((currentKey == 0 && currentKey2 == CreatureTagType.None) || (currentKey2 != CreatureTagType.None && currentKey2 == data.TagType))
		{
			HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(HeroListGrid, HeroPortraitComponent.SmallPrefabAuto);
			if (!(heroPortraitComponent == null))
			{
				heroPortraitComponent.Open(data, this, HeroPortraitComponent.EPortraitMode.Icon, visualStar, visualTranscend, 0, visualLevel);
				heroPortraitComponent.SetSelection(selection);
				heroPortraitComponent.SetLevel(show: true);
				heroPortraitComponent.ShowStar(show: true);
				heroPortraitComponent.SetDisable(flag2);
				string stringDirect = LocalizationManager.GetStringDirect("GUILD_NAME_NOT_POSSIBLE");
				heroPortraitComponent.SetRestricted(flag2, isSmall: true, stringDirect);
				_heroPortraitList.Add(heroPortraitComponent);
			}
		}
	}

	public void SelectHero(CreatureData heroData, bool isScrollTo = false, int visualLevel = 0, int visualStar = 0, int visualTranscend = 0)
	{
		if (heroData != null)
		{
			UnselectAllHeroPortraitComponent();
			SelectHeroPortraitComponentList(heroData, isScrollTo);
			SelectHeroPortraitLargeComponent(heroData, visualLevel, visualStar, visualTranscend);
			bool isSelect = _selectedHeroPortrait != null;
			UpdateConfirmButtonState(isSelect);
		}
	}

	public void OnClickSelectConfirm()
	{
		if (_iListener != null)
		{
			int itemIndex = ((_itemData != null) ? _itemData.Index : 0);
			int heroIndex = ((_selectedHeroPortrait != null && _selectedHeroPortrait.CreatureData != null) ? _selectedHeroPortrait.CreatureData.Index : 0);
			int visualLevel = ((_selectedHeroPortrait != null) ? _selectedHeroPortrait.VisualLevel : 0);
			int visualStar = ((_selectedHeroPortrait != null) ? _selectedHeroPortrait.VisualStar : 0);
			int visualTranscended = ((_selectedHeroPortrait != null) ? _selectedHeroPortrait.VisualTranscend : 0);
			_iListener.OnConfirmHeroGrowth(itemIndex, heroIndex, allGrowth: false, visualLevel, visualStar, visualTranscended, Close);
		}
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent comp)
	{
		if (comp == null || comp.CreatureData == null || comp.IsDisabled)
		{
			return;
		}
		CreatureData creatureData = comp.CreatureData;
		int visualLevel = comp.VisualLevel;
		int visualStar = comp.VisualStar;
		int visualTranscend = comp.VisualTranscend;
		if (_selectedHeroPortrait == comp)
		{
			if (_iListener != null)
			{
				_iListener.OpenHeroDetailView(creatureData.Index, visualLevel, visualStar, visualTranscend);
			}
		}
		else
		{
			SelectHero(creatureData, isScrollTo: false, visualLevel, visualStar, visualTranscend);
		}
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

	private void InitFilterComboBox()
	{
		ComboBoxFilterMain.Clear();
		ComboBoxFilterMain.AddItem(0, LocalizationManager.GetString(FilterSelect.FILTER_SELECT_ALL));
		ComboBoxFilterMain.OnChanged = OnChangeHeroList;
		ComboBoxFilterMain.Reposition();
		ComboBoxFilterMain.SelectFirstItem();
		ComboBoxFilterSub.Clear();
		ComboBoxFilterSub.AddItem(0, LocalizationManager.GetString(FilterSelect.FILTER_SELECT_ALL));
		foreach (CreatureTagType item in FilterHelper.GetListOrderCreatureTagType())
		{
			ComboBoxFilterSub.AddItem((int)item, LocalizationManager.GetString(item));
		}
		ComboBoxFilterSub.OnChanged = OnChangeHeroList;
		ComboBoxFilterSub.Reposition();
		ComboBoxFilterSub.SelectFirstItem();
	}

	private void UnselectAllHeroPortraitComponent()
	{
		FactoryBase.SafeReleaseComponent(ref _selectedHeroPortrait);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_heroPortraitList); i++)
		{
			HeroPortraitComponent heroPortraitComponent = _heroPortraitList[i];
			if (!(heroPortraitComponent == null))
			{
				heroPortraitComponent.SetSelection(select: false);
			}
		}
	}

	private void SelectHeroPortraitLargeComponent(CreatureData heroData, int visualLevel = 0, int visualStar = 0, int visualTranscend = 0)
	{
		_selectedHeroPortrait = HeroPortraitComponent.PrefabAuto.Factory.Create(SelectedHeroRoot);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_selectedHeroPortrait.transform);
		_selectedHeroPortrait.Open(heroData, this, HeroPortraitComponent.EPortraitMode.Basic, visualStar, visualTranscend, 0, visualLevel);
		_selectedHeroPortrait.ShowStar(show: true);
		_selectedHeroPortrait.SetLevel(show: true);
		_selectedHeroPortrait.ShowName(heroData.Name);
	}

	private void SelectHeroPortraitComponentList(CreatureData creatureData, bool isScrollTo)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_heroPortraitList); i++)
		{
			HeroPortraitComponent heroPortraitComponent = _heroPortraitList[i];
			if (!(heroPortraitComponent == null) && heroPortraitComponent.CreatureData != null && heroPortraitComponent.CreatureData.Index == creatureData.Index)
			{
				heroPortraitComponent.SetSelection(select: true);
				if (HeroListGrid != null && isScrollTo)
				{
					HeroListGrid.ScrollTo(heroPortraitComponent, -60f, 0f);
				}
				break;
			}
		}
	}

	private void UpdateInfoLabel(string key)
	{
		string stringDirect = LocalizationManager.GetStringDirect(key);
		UIHelper.SetLabelText(LabelConfirm, stringDirect);
	}

	private void UpdateConfirmButtonState(bool isSelect)
	{
		UIHelper.EnableButton(ButtonConfirm, isSelect, grayscaleIfDisable: true);
	}

	private void ClearHeroPortraitList()
	{
		FactoryBase.ReleaseAndClearList(_heroPortraitList);
	}
}
