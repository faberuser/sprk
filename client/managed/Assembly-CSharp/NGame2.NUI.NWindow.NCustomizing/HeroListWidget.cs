using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NPayShop;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NCustomizing;

public class HeroListWidget : WidgetBase, HeroPortraitComponent.Listener
{
	public interface Listener
	{
		HeroInfo GetHeroInfo();

		void SelectHero(int selectedHeroIndex);
	}

	public UIScrollGrid HeroListGrid;

	private List<HeroPortraitComponent> _heroPortraitList = new List<HeroPortraitComponent>();

	private HeroInfo SelectedHeroInfo
	{
		get
		{
			if (_renderFacadeController != null)
			{
				return _renderFacadeController.GetHeroInfo();
			}
			return null;
		}
	}

	public void Open()
	{
		SetActive(active: true);
		ClearHeroList();
		CreateHeroList();
	}

	public void Close()
	{
		SetActive(active: false);
		ClearHeroList();
	}

	private void SetActive(bool active)
	{
		base.gameObject.SetActive(active);
	}

	private void ClearHeroList()
	{
		FactoryBase.ReleaseAndClearList(_heroPortraitList);
	}

	private void CreateHeroList()
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
			CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(item.Index);
			if (cachedHeroInfo == null)
			{
				cachedHeroInfo = new CachedHeroInfo(item);
			}
			list.Add(cachedHeroInfo);
		}
		foreach (CachedHeroInfo item2 in list)
		{
			if (item2 != null)
			{
				HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(HeroListGrid, HeroPortraitComponent.SmallPrefabAuto);
				heroPortraitComponent.Open(item2, this, null, HeroPortraitComponent.EPortraitMode.Icon);
				bool flag = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(item2.HeroIndex) == null;
				if (flag)
				{
					heroPortraitComponent.SetNotOwnedSmall();
				}
				heroPortraitComponent.SetLevel(!flag);
				if (SelectedHeroInfo != null && SelectedHeroInfo.HeroIndex == item2.HeroIndex)
				{
					heroPortraitComponent.SetSelectSmall(select: true);
				}
				if (PayShopHelper.NewCostumeExists(item2.HeroIndex))
				{
					heroPortraitComponent.SetCostumeNewSmallTag();
				}
				_heroPortraitList.Add(heroPortraitComponent);
			}
		}
		UIHelper.RepositionGrid(HeroListGrid);
	}

	public void OnSelectHero(int heroIndex)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_heroPortraitList); i++)
		{
			if (SelectedHeroInfo.HeroIndex == _heroPortraitList[i].HeroInfo.HeroIndex)
			{
				_heroPortraitList[i].SetSelectSmall(select: false);
			}
			if (heroIndex == _heroPortraitList[i].HeroInfo.HeroIndex)
			{
				_heroPortraitList[i].SetSelectSmall(select: true);
			}
		}
	}

	public override void Open(ToggleType toggleType)
	{
		base.Open(toggleType);
	}

	public override void Reset()
	{
		Close();
	}

	public override void OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changeHeroInfo)
	{
		if (changeHeroInfo.ChangeType != NGame2.NAccount.UserHeroManager.HeroChangeType.Costume)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_heroPortraitList); i++)
		{
			HeroPortraitComponent heroPortraitComponent = _heroPortraitList[i];
			if (heroPortraitComponent.HeroInfo.HeroIndex == changeHeroInfo.HeroIndex)
			{
				heroPortraitComponent.SetHeroImage();
			}
		}
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent comp)
	{
		if (_renderFacadeController != null && !(comp == null) && comp.HeroInfo != null)
		{
			int heroIndex = comp.HeroInfo.HeroIndex;
			OnSelectHero(heroIndex);
			SetActive(active: false);
			_renderFacadeController.SetupHero(heroIndex);
		}
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo heroInfo)
	{
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent comp, bool isMine)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}
}
