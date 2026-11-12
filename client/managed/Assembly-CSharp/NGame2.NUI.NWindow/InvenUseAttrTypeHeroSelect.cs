using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class InvenUseAttrTypeHeroSelect : CommonInvenUseHeroSelect
{
	public const string PrefabPath = "Prefab2/Window/Inventory/InvenUseAttrTypeHeroSelect";

	public UIScrollGrid PhysicalHeroScrollGrid;

	public UIScrollGrid MagicalHeroScollGrid;

	public Transform HeroListRootTranform;

	public Transform RewardRootTransform;

	private List<EmptyHeroPortraitSmallComponent> _physicalHeroPortraitCompList = new List<EmptyHeroPortraitSmallComponent>();

	private List<EmptyHeroPortraitSmallComponent> _magicalHeroPortraitCompList = new List<EmptyHeroPortraitSmallComponent>();

	private bool CheckGetAllOwnedHeroReward
	{
		get
		{
			if (CheckRemainGetHero)
			{
				return false;
			}
			HeroSelectItemData heroSelectItemData = base.HeroSelectItemData;
			if (heroSelectItemData == null)
			{
				return false;
			}
			if (heroSelectItemData.AllOwnedRewardIndex == 0)
			{
				return false;
			}
			return true;
		}
	}

	private bool CheckRemainGetHero
	{
		get
		{
			List<int> heroIndexList = base.HeroIndexList;
			if (__Extension_Method_ICollection__.IsNullOrEmpty(heroIndexList))
			{
				return true;
			}
			for (int i = 0; i < heroIndexList.Count; i++)
			{
				if (GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroIndexList[i]) == null)
				{
					return true;
				}
			}
			return false;
		}
	}

	public void Open(Listener listener, int selectItemIndex, CreatureData selectHeroData)
	{
		Open(delegate
		{
			Init(listener, selectItemIndex, selectHeroData);
		}, null);
	}

	private void Init(Listener listener, int selectItemIndex, CreatureData selectedHeroData)
	{
		SprkTicketSearch.BeforeAttr(this);
		Init(listener, selectItemIndex);
		SetHeroList(selectedHeroData);
		UpdateButtonState();
		SprkTicketSearch.AfterAttr(this);
	}

	public override void Reset()
	{
		base.Reset();
		ClearHeroList();
		ClearRewardList();
		DeselectAllHeroPortraitComponent();
	}

	private void ClearHeroList()
	{
		FactoryBase.ReleaseAndClearList(_physicalHeroPortraitCompList);
		FactoryBase.ReleaseAndClearList(_magicalHeroPortraitCompList);
	}

	private void SetHeroList(CreatureData selectHeroData)
	{
		ClearHeroList();
		ClearRewardList();
		UIHelper.EnableGameObject(HeroListRootTranform, enable: true);
		UIHelper.EnableGameObject(RewardRootTransform, enable: false);
		if (CheckGetAllOwnedHeroReward)
		{
			UIHelper.EnableGameObject(HeroListRootTranform, enable: false);
			UIHelper.EnableGameObject(RewardRootTransform, enable: true);
			CreateAllOwnedReward();
			return;
		}
		List<int> heroIndexList = base.HeroIndexList;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(heroIndexList))
		{
			return;
		}
		HeroSelectItemData heroSelectItemData = base.HeroSelectItemData;
		if (heroSelectItemData == null)
		{
			return;
		}
		int num = 0;
		CreatureData creatureData = null;
		EmptyHeroPortraitSmallComponent.HeroInfo heroInfo = null;
		for (int i = 0; i < heroIndexList.Count; i++)
		{
			num = heroIndexList[i];
			creatureData = GameRoot.Instance.CreatureDataContainer.GetData(num);
			if (creatureData != null)
			{
				heroInfo = new EmptyHeroPortraitSmallComponent.HeroInfo
				{
					HeroIndex = num,
					HeroStar = heroSelectItemData.HeroStar,
					HeroTranscend = heroSelectItemData.HeroTranscend,
					IListener = this
				};
				if (creatureData.HeroAttrType == SkillAttrType.Physical)
				{
					AddComponent(heroInfo, ref PhysicalHeroScrollGrid, ref _physicalHeroPortraitCompList);
				}
				else if (creatureData.HeroAttrType == SkillAttrType.Magical)
				{
					AddComponent(heroInfo, ref MagicalHeroScollGrid, ref _magicalHeroPortraitCompList);
				}
			}
		}
		AddEmptyComponent(PhysicalHeroScrollGrid, _physicalHeroPortraitCompList);
		AddEmptyComponent(MagicalHeroScollGrid, _magicalHeroPortraitCompList);
		UIHelper.RepositionGrid(PhysicalHeroScrollGrid);
		UIHelper.RepositionGrid(MagicalHeroScollGrid);
		SelectHero(selectHeroData, isScrollTo: true);
	}

	private void AddComponent(EmptyHeroPortraitSmallComponent.HeroInfo heroInfo, ref UIScrollGrid grid, ref List<EmptyHeroPortraitSmallComponent> list)
	{
		if (SprkTicketSearch.MatchHeroInfo(this, heroInfo) && !(grid == null) && list != null)
		{
			EmptyHeroPortraitSmallComponent emptyHeroPortraitSmallComponent = UIHelper.AddGridItem(grid, EmptyHeroPortraitSmallComponent.PrefabAuto2);
			if (!(emptyHeroPortraitSmallComponent == null))
			{
				emptyHeroPortraitSmallComponent.Open(heroInfo);
				list.Add(emptyHeroPortraitSmallComponent);
			}
		}
	}

	private void AddEmptyComponent(UIScrollGrid grid, List<EmptyHeroPortraitSmallComponent> list)
	{
		if (list.Count < 4)
		{
			int num = 4 - list.Count;
			for (int i = 0; i < num; i++)
			{
				AddComponent(null, ref grid, ref list);
			}
		}
	}

	public override void SelectHero(CreatureData heroData, bool isScrollTo = false)
	{
		if (heroData != null)
		{
			DeselectAllHeroPortraitComponent();
			CreateSelectHeroPortrait(heroData);
			if (heroData.HeroAttrType == SkillAttrType.Physical)
			{
				SelectHeroPortraitComponent(heroData, _physicalHeroPortraitCompList, isScrollTo ? PhysicalHeroScrollGrid : null);
			}
			else if (heroData.HeroAttrType == SkillAttrType.Magical)
			{
				SelectHeroPortraitComponent(heroData, _magicalHeroPortraitCompList, isScrollTo ? MagicalHeroScollGrid : null);
			}
			UpdateButtonState();
		}
	}

	private void SelectHeroPortraitComponent(CreatureData creatureData, List<EmptyHeroPortraitSmallComponent> list, UIScrollGrid scollGrid)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return;
		}
		EmptyHeroPortraitSmallComponent emptyHeroPortraitSmallComponent = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			emptyHeroPortraitSmallComponent = list[i];
			if (!(emptyHeroPortraitSmallComponent == null) && !(emptyHeroPortraitSmallComponent.HeroPortraitComponent == null) && emptyHeroPortraitSmallComponent.HeroPortraitComponent.CreatureData == creatureData)
			{
				emptyHeroPortraitSmallComponent.HeroPortraitComponent.SetSelectSmall(select: true);
				if (scollGrid != null)
				{
					scollGrid.ScrollTo(emptyHeroPortraitSmallComponent, -60f, 0f);
				}
				break;
			}
		}
	}

	private void DeselectAllHeroPortraitComponent(ref List<EmptyHeroPortraitSmallComponent> list)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return;
		}
		FactoryBase.SafeReleaseComponent(ref _selectedHeroPortrait);
		EmptyHeroPortraitSmallComponent emptyHeroPortraitSmallComponent = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			emptyHeroPortraitSmallComponent = list[i];
			if (!(emptyHeroPortraitSmallComponent == null) && !(emptyHeroPortraitSmallComponent.HeroPortraitComponent == null))
			{
				emptyHeroPortraitSmallComponent.HeroPortraitComponent.SetSelectSmall(select: false);
			}
		}
	}

	private void DeselectAllHeroPortraitComponent()
	{
		DeselectAllHeroPortraitComponent(ref _physicalHeroPortraitCompList);
		DeselectAllHeroPortraitComponent(ref _magicalHeroPortraitCompList);
	}

	protected override void UpdateButtonState(bool isOtherReward = false)
	{
		if (CheckGetAllOwnedHeroReward)
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
		if (_iListener != null)
		{
			_iListener.OnSelectGetOtherReward(this);
		}
	}
}
