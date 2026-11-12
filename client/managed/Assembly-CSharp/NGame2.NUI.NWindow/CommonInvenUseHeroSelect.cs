using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public abstract class CommonInvenUseHeroSelect : BaseWindow, HeroPortraitComponent.Listener
{
	public interface Listener
	{
		void OnSelectHero(CommonInvenUseHeroSelect caller, int heroIndex, int heroLevel, int heroStar, int heroTranscend);

		void OpenHeroDetailView(ItemData itemData, HeroSelectItemData heroSelectItemData, int heroIndex);

		void OnSelectGetOtherReward(CommonInvenUseHeroSelect caller);
	}

	public UIScrollGrid RewardListGrid;

	public Transform SelectedHeroRoot;

	public UIButton GetHeroConfirmButton;

	public UIButton GetFragmentConfirmButton;

	public UIButton GetOtherRewardButton;

	protected Listener _iListener;

	protected HeroPortraitComponent _selectedHeroPortrait;

	protected List<Component> _rewardList;

	public ItemData ItemData { get; protected set; }

	public HeroSelectItemData HeroSelectItemData { get; private set; }

	protected List<int> HeroIndexList => HeroSelectItemData?.HeroIndices.ToList();

	public override void Reset()
	{
		base.Reset();
		ClearRewardList();
		FactoryBase.SafeReleaseComponent(ref _selectedHeroPortrait);
	}

	public override void Close()
	{
		base.Close();
	}

	protected void Init(Listener iListener, int selectItemIndex)
	{
		_iListener = iListener;
		ItemData = GameRoot.Instance.ItemDataContainer.GetData(selectItemIndex);
		if (ItemData != null)
		{
			HeroSelectItemData = GameRoot.Instance.HeroSelectItemDataContainer.GetData(ItemData.Code);
		}
	}

	protected void ClearRewardList()
	{
		if (_rewardList != null)
		{
			FactoryBase.ReleaseAndClearList(_rewardList);
			_rewardList = null;
		}
	}

	protected void AddHeroComponent(int heroIndex, ref UIScrollGrid grid, ref List<HeroPortraitComponent> list)
	{
		if (!(grid == null) && list != null)
		{
			CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroIndex);
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
			HeroSelectItemData heroSelectItemData = HeroSelectItemData;
			bool owned = cachedHeroInfo != null;
			HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(grid, HeroPortraitComponent.SmallPrefabAuto);
			heroPortraitComponent.Open(data, this, HeroPortraitComponent.EPortraitMode.Icon, heroSelectItemData.HeroStar, heroSelectItemData.HeroTranscend);
			heroPortraitComponent.SetOwned(owned);
			bool selectSmall = _selectedHeroPortrait != null && _selectedHeroPortrait.CreatureData != null && _selectedHeroPortrait.CreatureData.Index == data.Index;
			heroPortraitComponent.SetSelectSmall(selectSmall);
			list.Add(heroPortraitComponent);
		}
	}

	protected void CreateSelectHeroPortrait(CreatureData heroData)
	{
		if (heroData != null)
		{
			int index = heroData.Index;
			GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(index);
			HeroSelectItemData heroSelectItemData = HeroSelectItemData;
			if (heroSelectItemData != null)
			{
				_selectedHeroPortrait = HeroPortraitComponent.PrefabAuto.Factory.Create(SelectedHeroRoot);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_selectedHeroPortrait.transform);
				_selectedHeroPortrait.Open(heroData, this, HeroPortraitComponent.EPortraitMode.Basic, heroSelectItemData.HeroStar, heroSelectItemData.HeroTranscend);
				_selectedHeroPortrait.ShowCreatureName();
			}
		}
	}

	protected void DeselectAllHeroPortraitComponent(ref List<HeroPortraitComponent> list)
	{
		FactoryBase.SafeReleaseComponent(ref _selectedHeroPortrait);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
			{
				list[i].SetSelectSmall(select: false);
			}
		}
	}

	protected void SelectHeroPortraitComponent(CreatureData creatureData, List<HeroPortraitComponent> list, UIScrollGrid scollGrid)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			HeroPortraitComponent heroPortraitComponent = list[i];
			if (heroPortraitComponent.CreatureData == creatureData)
			{
				heroPortraitComponent.SetSelectSmall(select: true);
				if (scollGrid != null)
				{
					scollGrid.ScrollTo(heroPortraitComponent, -60f, 0f);
				}
				break;
			}
		}
	}

	protected void CreateAllOwnedReward()
	{
		if (!(RewardListGrid == null))
		{
			HeroSelectItemData heroSelectItemData = HeroSelectItemData;
			if (heroSelectItemData != null)
			{
				int allOwnedRewardIndex = heroSelectItemData.AllOwnedRewardIndex;
				_rewardList = UIHelper.CreateRewardItemIconComponentList(allOwnedRewardIndex, RewardListGrid, includeCurrency: true);
				UIHelper.RepositionGrid(RewardListGrid);
			}
		}
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
		if (heroPortrait == null)
		{
			return;
		}
		CreatureData creatureData = heroPortrait.CreatureData;
		if (creatureData == null)
		{
			return;
		}
		if (heroPortrait == _selectedHeroPortrait)
		{
			if (_iListener != null)
			{
				int index = creatureData.Index;
				HeroSelectItemData heroSelectItemData = HeroSelectItemData;
				if (heroSelectItemData != null)
				{
					_ = heroSelectItemData.HeroStar;
					_iListener.OpenHeroDetailView(ItemData, heroSelectItemData, index);
				}
			}
		}
		else
		{
			SelectHero(creatureData);
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

	public abstract void SelectHero(CreatureData heroData, bool isScrollTo = false);

	protected abstract void UpdateButtonState(bool isOtherReward = false);
}
