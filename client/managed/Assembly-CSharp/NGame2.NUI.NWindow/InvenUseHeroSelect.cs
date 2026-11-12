using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class InvenUseHeroSelect : CommonInvenUseHeroSelect
{
	public const string PrefabPath = "Prefab2/Window/Inventory/InvenUseHeroSelect";

	[SerializeField]
	private SelectFilter _selectFilter;

	[SerializeField]
	private UILabel _lbNoneHero;

	public UIScrollGrid HeroListGrid;

	public UILabel Label_TouchHero;

	public UILabel Label_CanGetOtherReward;

	private List<HeroPortraitComponent> _heroPortraitList = new List<HeroPortraitComponent>();

	public void Open(Listener listener, int selectedItemIndex)
	{
		Open(delegate
		{
			Init(listener, selectedItemIndex);
		}, null);
	}

	public void Init(Listener listener, int selectItemIndex, CreatureData selectedHeroData = null)
	{
		base.Init(listener, selectItemIndex);
		if (_selectFilter != null)
		{
			_selectFilter.InitFilter(SelectFilter.SelectType.SELECT_HERO, delegate
			{
				CreateHeroPortraitList(selectedHeroData);
			});
		}
		else
		{
			UIHelper.EnableGameObject(_selectFilter, enable: false);
			CreateHeroPortraitList(selectedHeroData);
		}
	}

	public override void Reset()
	{
		base.Reset();
		ClearHeroPortrait();
		DeselectAllHeroPortraitComponent(ref _heroPortraitList);
		UpdateButtonState();
	}

	private void SetRewardLabel(bool isExist)
	{
		UIHelper.EnableGameObject(Label_TouchHero, isExist);
		UIHelper.SetLabelText(Label_CanGetOtherReward, "");
		if (!isExist)
		{
			string stringDirect = LocalizationManager.GetStringDirect(base.HeroSelectItemData.AllOwnedRewardDesc);
			UIHelper.SetLabelText(Label_CanGetOtherReward, stringDirect);
		}
	}

	private void ClearHeroPortrait()
	{
		FactoryBase.ReleaseAndClearList(_heroPortraitList);
	}

	private void CreateHeroPortraitList(CreatureData selectedHeroData = null)
	{
		ClearHeroPortrait();
		ClearRewardList();
		if (base.ItemData != null && base.HeroSelectItemData != null)
		{
			bool flag = false;
			if (__Extension_Method_ICollection__.COUNT(base.HeroSelectItemData.HeroIndices) != 0)
			{
				flag = CheckSetHeroIndices();
			}
			UIHelper.EnableGameObject(HeroListGrid, flag);
			UIHelper.EnableGameObject(RewardListGrid, !flag);
			UIHelper.EnableGameObject(_lbNoneHero, _heroPortraitList.Count <= 0 && flag);
			SetRewardLabel(flag);
			UpdateButtonState(!flag);
			if (flag)
			{
				UIHelper.RepositionGrid(HeroListGrid);
				SelectHero(selectedHeroData, isScrollTo: true);
			}
			else
			{
				CreateAllOwnedReward();
			}
		}
	}

	private bool CheckSetHeroIndices()
	{
		bool num = base.HeroSelectItemData.AllOwnedRewardIndex == 0;
		bool result = true;
		int[] heroIndices;
		if (num)
		{
			heroIndices = base.HeroSelectItemData.HeroIndices;
			foreach (int heroIndex in heroIndices)
			{
				AddHeroByFilter(heroIndex);
			}
			return result;
		}
		result = false;
		heroIndices = base.HeroSelectItemData.HeroIndices;
		foreach (int key in heroIndices)
		{
			if (GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(key) == null)
			{
				result = true;
				break;
			}
		}
		if (result)
		{
			heroIndices = base.HeroSelectItemData.HeroIndices;
			foreach (int heroIndex2 in heroIndices)
			{
				AddHeroByFilter(heroIndex2);
			}
		}
		return result;
	}

	private void AddHeroByFilter(int heroIndex)
	{
		if (!SprkTicketSearch.MatchHero(this, heroIndex))
		{
			return;
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		if (data == null || data.OpenType == CreatureOpenType.ChapterReward)
		{
			return;
		}
		if (_selectFilter == null)
		{
			AddHeroComponent(heroIndex, ref HeroListGrid, ref _heroPortraitList);
		}
		else if (_selectFilter.MainComboBoxCurrentKey == SelectFilter.ComboBoxType.ALL || _selectFilter.MainComboBoxCurrentKey == SelectFilter.ComboBoxType.OWNED || _selectFilter.MainComboBoxCurrentKey == SelectFilter.ComboBoxType.NOT_OWNED)
		{
			bool flag = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroIndex) != null;
			if ((_selectFilter.MainComboBoxCurrentKey != SelectFilter.ComboBoxType.OWNED || flag) && (_selectFilter.MainComboBoxCurrentKey != SelectFilter.ComboBoxType.NOT_OWNED || !flag) && (_selectFilter.SubComboBoxCurrentKey == 0 || _selectFilter.SubComboBoxCurrentKey == (int)data.TagType))
			{
				AddHeroComponent(heroIndex, ref HeroListGrid, ref _heroPortraitList);
			}
		}
	}

	public override void SelectHero(CreatureData heroData, bool isScrollTo = false)
	{
		if (heroData != null)
		{
			DeselectAllHeroPortraitComponent(ref _heroPortraitList);
			CreateSelectHeroPortrait(heroData);
			SelectHeroPortraitComponent(heroData, _heroPortraitList, isScrollTo ? HeroListGrid : null);
			UpdateButtonState();
		}
	}

	protected override void UpdateButtonState(bool isOtherReward = false)
	{
		if (isOtherReward)
		{
			UIHelper.EnableGameObject(GetOtherRewardButton, enable: true);
			UIHelper.EnableGameObject(GetHeroConfirmButton, enable: false);
			UIHelper.EnableGameObject(GetFragmentConfirmButton, enable: false);
			return;
		}
		bool num = base.ItemData != null && base.HeroSelectItemData != null && base.HeroSelectItemData.AllOwnedRewardIndex == 0;
		bool flag = false;
		if (_selectedHeroPortrait != null && _selectedHeroPortrait.CreatureData != null)
		{
			int index = _selectedHeroPortrait.CreatureData.Index;
			flag = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(index) != null;
		}
		UIHelper.EnableGameObject(GetOtherRewardButton, enable: false);
		if (num)
		{
			UIHelper.EnableGameObject(GetHeroConfirmButton, !flag);
			UIHelper.EnableGameObject(GetFragmentConfirmButton, flag);
			UIHelper.EnableButton(GetHeroConfirmButton, _selectedHeroPortrait != null);
			UIHelper.EnableButton(GetFragmentConfirmButton, _selectedHeroPortrait != null);
		}
		else
		{
			UIHelper.EnableGameObject(GetFragmentConfirmButton, enable: false);
			UIHelper.EnableGameObject(GetHeroConfirmButton, enable: true);
			UIHelper.EnableButton(GetHeroConfirmButton, !flag && _selectedHeroPortrait != null);
		}
	}

	public void OnClickSelectConfirm()
	{
		if (!(_selectedHeroPortrait == null) && _selectedHeroPortrait.CreatureData != null)
		{
			int index = _selectedHeroPortrait.CreatureData.Index;
			HeroSelectItemData heroSelectItemData = base.HeroSelectItemData;
			if (heroSelectItemData != null && _iListener != null)
			{
				_iListener.OnSelectHero(this, index, heroSelectItemData.HeroLevel, heroSelectItemData.HeroStar, heroSelectItemData.HeroTranscend);
			}
		}
	}

	public void OnClickGetOtherReward()
	{
		if (__Extension_Method_ICollection__.COUNT(_heroPortraitList) <= 0 && !__Extension_Method_ICollection__.IsNullOrEmpty(_rewardList) && _iListener != null)
		{
			_iListener.OnSelectGetOtherReward(this);
		}
	}
}
