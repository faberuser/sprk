using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class HeroSelect : BaseWindow, HeroPortraitComponent.Listener
{
	public interface Listener : HeroFullSceneViewBase.Listener
	{
		void OnSelectHero(int heroIndex);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Hero/HeroSelect";

	public UIScrollGrid HeroListGrid;

	private Listener _listener;

	private Action<int> _onSelectHero;

	private List<HeroPortraitComponent> _heroPortraitList = new List<HeroPortraitComponent>();

	public static void OpenPopup(Action<int> onSelectHero, Func<CreatureData, bool> predicate = null)
	{
		WindowPrefab<HeroSelect> windowPrefab = new WindowPrefab<HeroSelect>("Resources/Prefab2/Window/Hero/HeroSelect", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.Open(onSelectHero, predicate);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void Open(Action<int> OnSelectHero, Func<CreatureData, bool> predicate)
	{
		base.Open();
		_onSelectHero = OnSelectHero;
		CreateHeroList(predicate);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_heroPortraitList);
	}

	private void CreateHeroList(Func<CreatureData, bool> predicate)
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
			if (predicate == null || Vespa_System_Action_Extension_Method.fire(predicate, item))
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
				HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(HeroListGrid, HeroPortraitComponent.SmallPrefabAuto);
				heroPortraitComponent.Open(item2, this, null, HeroPortraitComponent.EPortraitMode.Icon);
				bool flag = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(item2.HeroIndex) == null;
				if (flag)
				{
					heroPortraitComponent.SetNotOwnedSmall();
				}
				heroPortraitComponent.SetLevel(!flag);
				_heroPortraitList.Add(heroPortraitComponent);
			}
		}
		UIHelper.RepositionGrid(HeroListGrid);
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
		Vespa_System_Action_Extension_Method.fire(_onSelectHero, heroPortrait.HeroInfo.HeroIndex);
		Close();
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
}
