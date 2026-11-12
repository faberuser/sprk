using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class InvenUseHeroCostumeSelect : BaseWindow, HeroPortraitComponent.Listener
{
	public interface Listener
	{
		void OnSelectCostume(int useItemIndex, int costumeIndex);

		void OpenHeroCostumeView(int heroIndex, int costumeIndex);

		void OnSelectGetOtherReward(int useItemIndex);
	}

	public const string PrefabPath = "Prefab2/Window/Inventory/InvenUseHeroCostumeSelect";

	public UIScrollGrid HeroListGrid;

	public UIScrollGrid RewardListGrid;

	public UILabel Label_TouchHero;

	public UILabel Label_CanGetOtherReward;

	public Transform SelectedHeroRoot;

	public UIButton GetHeroCostumeButton;

	public UIButton Button_GetOtherReward;

	public UILabel Label_Title;

	[SerializeField]
	private SelectFilter _selectFilter;

	[SerializeField]
	private UILabel _lbNoneCostume;

	[SerializeField]
	private UILabel LbRewardRankingPoint;

	private List<HeroPortraitComponent> _heroPortraitList = new List<HeroPortraitComponent>();

	private List<Component> _rewardList;

	private HeroPortraitComponent _selectedHeroPortrait;

	private Listener _listener;

	public ItemData<CostumeSelectItemData> ItemData { get; private set; }

	public void Open(Listener listener, int selectedItemIndex)
	{
		Open(delegate
		{
			Init(listener, selectedItemIndex);
		}, null);
	}

	public void Init(Listener listener, int selectedItemIndex)
	{
		_listener = listener;
		ItemData = GameRoot.Instance.ItemDataContainer.GetData<CostumeSelectItemData>(selectedItemIndex);
		SetTitle();
		if (_selectFilter != null)
		{
			_selectFilter.InitFilter(SelectFilter.SelectType.SELECT_COSTUME, CreateHeroPortrait, ItemData, useSubFilter: true, useSecondaryMainFilter: true);
		}
		else
		{
			UIHelper.EnableGameObject(_selectFilter, enable: false);
		}
	}

	public override void Reset()
	{
		base.Reset();
		ClearHeroPortrait();
		ClearRewardList();
		DeselectHero();
	}

	public void OnClickSelectConfirm()
	{
		if (_selectedHeroPortrait == null)
		{
			return;
		}
		int heroIndex = _selectedHeroPortrait.HeroInfo.HeroIndex;
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroIndex);
		if (cachedHeroInfo != null)
		{
			int costumeIndex = cachedHeroInfo.CostumeIndex;
			CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex);
			if (data != null && data.CostumeType == EHeroCostumeType.Legend)
			{
				string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
				string stringDirect = LocalizationManager.GetStringDirect("COSTUME_LEGEND_UNEQUIP_NOTICE_SELECT");
				MessagePopup3Button.Open(text, stringDirect, null, delegate
				{
					OnSelectCostume();
				}, NShared.NLocalization.Common.COMMON_NO, NShared.NLocalization.Common.COMMON_YES);
				return;
			}
		}
		OnSelectCostume();
	}

	private void OnSelectCostume()
	{
		if (_listener != null)
		{
			int costumeIndex = _selectedHeroPortrait.HeroInfo.CostumeIndex;
			_listener.OnSelectCostume(ItemData.Index, costumeIndex);
		}
	}

	public void OnClickGetOtherReward()
	{
		if (__Extension_Method_ICollection__.COUNT(_heroPortraitList) <= 0 && _rewardList != null && _listener != null)
		{
			_listener.OnSelectGetOtherReward(ItemData.Index);
		}
	}

	private void SetTitle()
	{
		if (ItemData != null && ItemData.SubData != null)
		{
			if (ItemData.SubData.CostumeSelectItemType == ECostumeSelectItemType.Legend)
			{
				UIHelper.SetLabelText(Label_Title, LocalizationManager.GetStringDirect("COSTUMECHOICE_TITLE_LEGEND"));
			}
			else
			{
				UIHelper.SetLabelText(Label_Title, LocalizationManager.GetStringDirect("COSTUMECHOICE_TITLE"));
			}
		}
	}

	private void ClearRewardList()
	{
		if (_rewardList != null)
		{
			FactoryBase.ReleaseAndClearList(_rewardList);
			_rewardList = null;
		}
	}

	private void ClearHeroPortrait()
	{
		FactoryBase.ReleaseAndClearList(_heroPortraitList);
	}

	private void CreateHeroPortrait()
	{
		ClearHeroPortrait();
		ClearRewardList();
		if (ItemData != null && ItemData.SubData != null)
		{
			bool flag = false;
			if (ItemData.SubData.CostumeIndices != null)
			{
				flag = CheckSetCostumeIndices();
			}
			if (ItemData.SubData.CostumeCategories != null)
			{
				flag = CheckSetCostumeCategories();
			}
			UIHelper.EnableGameObject(Label_TouchHero, _heroPortraitList.Count > 0);
			UIHelper.EnableGameObject(GetHeroCostumeButton, _heroPortraitList.Count > 0);
			UIHelper.EnableGameObject(Button_GetOtherReward, !flag);
			UIHelper.EnableGameObject(_lbNoneCostume, _heroPortraitList.Count <= 0 && flag);
			SetRewardLabel(flag);
			if (flag)
			{
				UIHelper.EnableGameObject(HeroListGrid, enable: true);
				UIHelper.RepositionGrid(HeroListGrid);
				return;
			}
			UIHelper.EnableGameObject(HeroListGrid, enable: false);
			UIHelper.EnableGameObject(RewardListGrid, enable: true);
			int allOwnedRewardIndex = ItemData.SubData.AllOwnedRewardIndex;
			_rewardList = UIHelper.CreateRewardItemIconComponentList(allOwnedRewardIndex, RewardListGrid, includeCurrency: true);
			UIHelper.RepositionGrid(RewardListGrid);
		}
	}

	private void SetRewardLabel(bool isExist)
	{
		UIHelper.EnableGameObject(LbRewardRankingPoint, enable: false);
		UIHelper.SetLabelText(Label_CanGetOtherReward, "");
		if (isExist)
		{
			return;
		}
		string value = GameRoot.Instance.RewardDataContainer.GetData(ItemData.SubData.AllOwnedRewardIndex).ItemCode1;
		try
		{
			if ((RewardType)Enum.Parse(typeof(RewardType), value) == RewardType.RankingPoint)
			{
				UIHelper.EnableGameObject(LbRewardRankingPoint, enable: true);
				return;
			}
			string stringDirect = LocalizationManager.GetStringDirect(ItemData.SubData.AllOwnedRewardDesc);
			UIHelper.SetLabelText(Label_CanGetOtherReward, stringDirect);
		}
		catch
		{
			string stringDirect2 = LocalizationManager.GetStringDirect(ItemData.SubData.AllOwnedRewardDesc);
			UIHelper.SetLabelText(Label_CanGetOtherReward, stringDirect2);
		}
	}

	private bool CheckSetCostumeIndices()
	{
		bool result = false;
		int[] costumeIndices = ItemData.SubData.CostumeIndices;
		foreach (int num in costumeIndices)
		{
			if (GameRoot.Instance.CostumeDataContainer.GetData(num) != null && GlobalAutoBehaviour<AccountManager>.instance.CostumeManager.Get(num) == null)
			{
				result = true;
				AddHeroByFilter(num);
			}
		}
		return result;
	}

	private bool CheckSetCostumeCategories()
	{
		bool result = false;
		string[] costumeCategories = ItemData.SubData.CostumeCategories;
		foreach (string text in costumeCategories)
		{
			foreach (CostumeData item in GameRoot.Instance.CostumeDataContainer.GetDataByGroup(text))
			{
				if (item != null && !item.IsBonusCostume() && GlobalAutoBehaviour<AccountManager>.instance.CostumeManager.Get(item.CostumeIndex) == null)
				{
					result = true;
					AddHeroByFilter(item.CostumeIndex);
				}
			}
		}
		return result;
	}

	private void AddHeroByFilter(int costumeIndex)
	{
		if (!SprkTicketSearch.MatchCostume(this, costumeIndex))
		{
			return;
		}
		CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex);
		if (data == null)
		{
			return;
		}
		if (_selectFilter != null)
		{
			switch (_selectFilter.MainComboBoxCurrentKey)
			{
			case SelectFilter.ComboBoxType.ALL:
			case SelectFilter.ComboBoxType.CREATURE:
			{
				CreatureData data2 = GameRoot.Instance.CreatureDataContainer.GetData(data.HeroIndex);
				CreatureTagType subComboBoxCurrentKey = (CreatureTagType)_selectFilter.SubComboBoxCurrentKey;
				if (_selectFilter.SubComboBoxCurrentKey == 0 || subComboBoxCurrentKey == data2.TagType)
				{
					CreatePortrait(data.HeroIndex, data.CostumeIndex);
				}
				break;
			}
			case SelectFilter.ComboBoxType.COSTUME:
				if (_selectFilter.SecondaryMainComboBoxCurrentKey == -1)
				{
					CreatePortrait(data.HeroIndex, data.CostumeIndex);
				}
				else
				{
					if (data.Group == null)
					{
						break;
					}
					CostumeGroupData dataByGroupName = GameRoot.Instance.CostumeGroupDataContainer.GetDataByGroupName(data.Group);
					if (_selectFilter.SubComboBoxCurrentKey == 0)
					{
						if (GameRoot.Instance.CostumeGroupDataContainer.GetSelectItemGroupKey(dataByGroupName.SelectItemGroup) == _selectFilter.SecondaryMainComboBoxCurrentKey)
						{
							CreatePortrait(data.HeroIndex, data.CostumeIndex);
						}
					}
					else if (dataByGroupName.CostumeGroupIndex == _selectFilter.SubComboBoxCurrentKey)
					{
						CreatePortrait(data.HeroIndex, data.CostumeIndex);
					}
				}
				break;
			case (SelectFilter.ComboBoxType)1:
				break;
			}
		}
		else
		{
			CreatePortrait(data.HeroIndex, data.CostumeIndex);
		}
	}

	private void CreatePortrait(int heroIndex, int costumeIndex)
	{
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroIndex);
		if (cachedHeroInfo == null)
		{
			CreateNotOwnedHeroPortrait(heroIndex, costumeIndex);
			return;
		}
		CachedHeroInfo cachedHeroInfo2 = JsonMarshaler.Clone(cachedHeroInfo);
		cachedHeroInfo2.CostumeIndex = costumeIndex;
		CreateHeroPortrait(cachedHeroInfo2);
	}

	private void CreateNotOwnedHeroPortrait(int heroIndex, int costumeIndex)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		if (data == null)
		{
			VespaDebug.LogError("Creature Data is null, creature Index = " + heroIndex);
			return;
		}
		HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(HeroListGrid, HeroPortraitComponent.SmallPrefabAuto);
		heroPortraitComponent.Open(data, null, HeroPortraitComponent.EPortraitMode.Icon, 0, 0, costumeIndex);
		heroPortraitComponent.SetNotOwnedSmall();
		_heroPortraitList.Add(heroPortraitComponent);
	}

	private void CreateHeroPortrait(CachedHeroInfo heroInfo)
	{
		HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(HeroListGrid, HeroPortraitComponent.SmallPrefabAuto);
		heroPortraitComponent.Open(heroInfo, this, null, HeroPortraitComponent.EPortraitMode.Icon);
		_heroPortraitList.Add(heroPortraitComponent);
	}

	private void DeselectHero()
	{
		FactoryBase.SafeReleaseComponent(ref _selectedHeroPortrait);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_heroPortraitList); i++)
		{
			_heroPortraitList[i].SetSelectSmall(select: false);
		}
		UpdateButtonState();
	}

	private void SelectHero(CachedHeroInfo heroInfo)
	{
		if (heroInfo != null)
		{
			DeselectHero();
			_selectedHeroPortrait = HeroPortraitComponent.PrefabAuto.Factory.Create(SelectedHeroRoot);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_selectedHeroPortrait.transform);
			int costumeIndex = heroInfo.CostumeIndex;
			Debug.Log(costumeIndex);
			CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex);
			_selectedHeroPortrait.Open(heroInfo, this);
			string text = ((data == null) ? "(error)" : data.CostumeName);
			_selectedHeroPortrait.ShowName(text);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_heroPortraitList); i++)
			{
				HeroPortraitComponent heroPortraitComponent = _heroPortraitList[i];
				bool selectSmall = Vespa_Common_Class_Extension_Method.EQUALS(heroPortraitComponent.HeroInfo, heroInfo) && heroInfo.CostumeIndex == data.CostumeIndex;
				heroPortraitComponent.SetSelectSmall(selectSmall);
			}
			UpdateButtonState();
		}
	}

	private void UpdateButtonState()
	{
		UIHelper.EnableButton(GetHeroCostumeButton, _selectedHeroPortrait != null);
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
		if (heroPortrait == null)
		{
			return;
		}
		CachedHeroInfo heroInfo = heroPortrait.HeroInfo;
		if (heroInfo == null)
		{
			return;
		}
		if (Vespa_Common_Class_Extension_Method.EQUALS(heroPortrait, _selectedHeroPortrait))
		{
			if (_listener != null)
			{
				int heroIndex = heroInfo.HeroIndex;
				int costumeIndex = heroInfo.CostumeIndex;
				_listener.OpenHeroCostumeView(heroIndex, costumeIndex);
			}
		}
		else
		{
			SelectHero(heroInfo);
		}
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent heroPortrait, bool isMine)
	{
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo heroInfo)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}
}
