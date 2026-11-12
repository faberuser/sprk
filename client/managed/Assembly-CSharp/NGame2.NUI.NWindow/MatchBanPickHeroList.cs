using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class MatchBanPickHeroList : EquipEnhanceBase, HeroEquipListComponent.Listener
{
	public const string PrefabPath = "Resources/Prefab2/Window/Match/MatchBanPickHeroList";

	public UILabel Desc;

	public UIScrollGrid HeroGrid;

	private int selectedHeroIndex;

	public override void Open()
	{
		base.Open();
		InitBase();
	}

	protected override void CreateHeroEquipList()
	{
		if (!(HeroEquipItemGrid == null))
		{
			FactoryBase.ReleaseAndClearList(_heroEquipListComponentList);
			CachedHeroInfo[] availableHeroList = GetAvailableHeroList();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(availableHeroList); i++)
			{
				CachedHeroInfo heroInfo = availableHeroList[i];
				AddHeroEquipListComponent(heroInfo);
			}
			UIHelper.RepositionGrid(HeroEquipItemGrid);
			UIHelper.EnableGameObject(EmptyHeroEquipRoot, __Extension_Method_ICollection__.COUNT(_heroEquipListComponentList) == 0);
			string stringDirect = LocalizationManager.GetStringDirect("MATCH_PICKBAN_HERO_INFO_POPUP_DESC", availableHeroList.Length);
			UIHelper.SetLabelText(Desc, stringDirect);
		}
	}

	private void AddHeroEquipListComponent(CachedHeroInfo heroInfo)
	{
		HeroEquipListComponent heroEquipListComponent = UIHelper.AddGridItem(HeroEquipItemGrid, HeroEquipListComponent.PrefabAuto);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(heroEquipListComponent.transform);
		heroEquipListComponent.Open(heroInfo, this, selectable: true);
		_heroEquipListComponentList.Add(heroEquipListComponent);
	}

	public void OnClickHeroInfo()
	{
		if (SceneAutoBehaviour<MatchManagement>.isValidInstance)
		{
			Close();
			List<CachedHeroInfo> heroInfoList = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: true);
			CachedHeroInfo cachedHeroInfo = heroInfoList.Find((CachedHeroInfo e) => e.HeroIndex == selectedHeroIndex);
			if (cachedHeroInfo == null)
			{
				cachedHeroInfo = heroInfoList.First();
			}
			SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyHeroAsync(cachedHeroInfo, Hero3DViewPanel.EToggle.Equip).Commit();
		}
	}

	private CachedHeroInfo[] GetAvailableHeroList()
	{
		CachedHeroInfo[] array = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ToArray();
		int limitHeroLevel = GameRoot.Instance.ConstantDataContainer.GetValueInt("MatchingBanPickHeroLevelMin");
		int limitHeroStar = GameRoot.Instance.ConstantDataContainer.GetValueInt("MatchingBanPickHeroStarMin");
		CachedHeroInfo[] array2 = Array.FindAll(array, (CachedHeroInfo e) => e.Level >= limitHeroLevel && e.Star >= limitHeroStar);
		Array.Sort(array2, (CachedHeroInfo leftHeroinfo, CachedHeroInfo rightHeroInfo) => new HeroComparer().Compare(leftHeroinfo, rightHeroInfo));
		return array2;
	}

	void HeroEquipListComponent.Listener.OnClick(HeroPortraitComponent component)
	{
	}

	void HeroEquipListComponent.Listener.OnClick(ItemIconComponent component)
	{
		OnClickItemIconComponent(component);
	}

	void HeroEquipListComponent.Listener.OnClickThis(int heroIndex)
	{
		foreach (HeroEquipListComponent heroEquipListComponent in _heroEquipListComponentList)
		{
			heroEquipListComponent.UnSelect();
		}
		selectedHeroIndex = heroIndex;
	}
}
