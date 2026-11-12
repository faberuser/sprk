using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NAchievement;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NHeroPortraitSub;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.Mission;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class AchievementList : LobbyFullWindow, AchievementComponent.Listener, HeroPortraitComponent.Listener
{
	public enum GroupAchievementStatus
	{
		Completed,
		Cleared,
		NotCleared
	}

	[Serializable]
	public class AchievementCategory
	{
		public UIToggle Toggle;

		public AchievementType AchievementType;

		public AchievementCategoryType CategoryType;

		public UIWidget WidgetClearedCount;

		public UILabel Label_ClearedCount;

		public void SetActive(int ClearCount)
		{
			UIHelper.EnableGameObject(WidgetClearedCount, ClearCount > 0);
			if (ClearCount > 0)
			{
				UIHelper.SetLabelText(Label_ClearedCount, ClearCount);
			}
		}
	}

	[Serializable]
	public class ClearedCountWidget
	{
		public UIWidget WidgetClearedCount;

		public UILabel Label_ClearedCount;

		public void SetActive(int ClearCount)
		{
			UIHelper.EnableGameObject(WidgetClearedCount, ClearCount > 0);
			if (ClearCount > 0)
			{
				UIHelper.SetLabelText(Label_ClearedCount, ClearCount);
			}
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Achievement/AchievementList";

	[Header("[AchievementList]")]
	public UIScrollGrid HeroListGrid;

	public HeroPortraitComponent.PrefabSelector PortraitSelector = new HeroPortraitComponent.PrefabSelector(HeroPortraitComponent.PrefabSelector.EPrefabType.Default);

	private AchievementType _curType = AchievementType.Hero;

	private AchievementCategoryType _curCategoryType;

	private List<AchievementComponent> _ListAchievementComponent = new List<AchievementComponent>();

	private List<HeroPortraitComponent> _ListAchievementHeroPortraitComponent = new List<HeroPortraitComponent>();

	private bool _isDirty;

	public void Init()
	{
		RefreshMyLayer();
		HeroListGrid.Grid.sorting = UIGrid.Sorting.Custom;
		HeroListGrid.Grid.onCustomSort = SortItem;
		SetupListTask();
	}

	private void RefreshMyLayer()
	{
		if (GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpenedUI3DUI())
		{
			LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI3DUI);
			ChangeLayerRecursive(layerInfo);
		}
		else
		{
			LayerInfo layerInfo2 = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI);
			ChangeLayerRecursive(layerInfo2);
		}
	}

	private void Clear()
	{
		_isDirty = false;
		ResetListComponent();
	}

	public override void Reset()
	{
		base.Reset();
		Clear();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		Clear();
	}

	public override void Close()
	{
		if (SceneAutoBehaviour<MissionManagement>.isValidInstance)
		{
			SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.CloseMissionMainSubWnds();
		}
		base.Close();
	}

	public void OnChangeHero()
	{
		_curType = AchievementType.Hero;
		_curCategoryType = AchievementCategoryType.None;
		RefreshList();
	}

	public void OnChangeToggle()
	{
		AchievementType achievementType = AchievementType.General;
		AchievementCategoryType achievementCategoryType = AchievementCategoryType.None;
		if (achievementType != _curType || achievementCategoryType != _curCategoryType)
		{
			_curType = achievementType;
			_curCategoryType = achievementCategoryType;
			RefreshList();
		}
	}

	public void SetDirty()
	{
		_isDirty = true;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_isDirty)
		{
			RefreshList();
			_isDirty = false;
		}
	}

	public void RefreshList()
	{
		ResetListComponent();
		SetupListTask();
	}

	public void UpdateAchievementHero(int heroIndex)
	{
		if (_curType == AchievementType.Hero)
		{
			HeroPortraitComponent heroPortraitComponent = _ListAchievementHeroPortraitComponent.Find((HeroPortraitComponent comp) => comp.HeroInfo != null && comp.HeroInfo.HeroIndex == heroIndex);
			if (!(heroPortraitComponent == null))
			{
				heroPortraitComponent.Close();
				CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroIndex);
				heroPortraitComponent.Open(heroInfo, this);
				heroPortraitComponent.SetupAchievement(SingletonAuto<AchievementChecker>.instance);
			}
		}
	}

	private void GenerateAchievementList()
	{
		AchievementType curType = _curType;
		if (curType != AchievementType.General && curType == AchievementType.Hero)
		{
			GenerateAchievementHeroList();
		}
	}

	private void SetupListTask()
	{
		UIHelper.EnableGameObject(HeroListGrid, _curType == AchievementType.Hero);
		GenerateAchievementList();
	}

	private void ResetListComponent()
	{
		FactoryBase.ReleaseAndClearList(_ListAchievementComponent);
		FactoryBase.ReleaseAndClearList(_ListAchievementHeroPortraitComponent);
	}

	private void GenerateAchievementHeroList()
	{
		IList<CreatureData> allData = GameRoot.Instance.CreatureDataContainer.GetAllData();
		List<int> list = new List<int>();
		for (int i = 0; i < allData.Count; i++)
		{
			CreatureData creatureData = allData[i];
			CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(creatureData.Index);
			if (cachedHeroInfo != null)
			{
				CreateAchievementHeroComponent(cachedHeroInfo);
			}
			else if (creatureData.OpenType != CreatureOpenType.None && creatureData.OpenType != CreatureOpenType.CommingSoon)
			{
				list.Add(creatureData.Index);
			}
		}
		list.ForEach(delegate(int heroIndex)
		{
			CreateNotOwnedAchievementHeroComponent(heroIndex);
		});
		UIHelper.RepositionGrid(HeroListGrid);
	}

	private void CreateAchievementHeroComponent(CachedHeroInfo heroInfo)
	{
		PrefabAuto<HeroPortraitComponent> prefabAuto = PortraitSelector.PrefabAuto;
		HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(HeroListGrid, prefabAuto);
		heroPortraitComponent.Open(heroInfo, this);
		heroPortraitComponent.ShowCreatureName();
		heroPortraitComponent.SetupAchievement(SingletonAuto<AchievementChecker>.instance);
		_ListAchievementHeroPortraitComponent.Add(heroPortraitComponent);
	}

	private void CreateNotOwnedAchievementHeroComponent(int HeroIndex)
	{
		PrefabAuto<HeroPortraitComponent> prefabAuto = PortraitSelector.PrefabAuto;
		HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(HeroListGrid, prefabAuto);
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(HeroIndex);
		if (data == null)
		{
			VespaDebug.LogError("Creature Data is not found, CreatureIndex = " + HeroIndex);
			return;
		}
		heroPortraitComponent.Open(data, null);
		heroPortraitComponent.ShowCreatureName();
		heroPortraitComponent.SetGrayScale(grayScale: true);
		_ListAchievementHeroPortraitComponent.Add(heroPortraitComponent);
	}

	private int SortItem(Transform a, Transform b)
	{
		HeroPortraitComponent component = a.GetComponent<HeroPortraitComponent>();
		HeroPortraitComponent component2 = b.GetComponent<HeroPortraitComponent>();
		if (component == null || component2 == null)
		{
			if (component != null)
			{
				return -1;
			}
			if (component2 != null)
			{
				return 1;
			}
			return 0;
		}
		AchievementClearedSubComponent subComponent = component.SubCompContainer.GetSubComponent<AchievementClearedSubComponent>();
		AchievementClearedSubComponent subComponent2 = component2.SubCompContainer.GetSubComponent<AchievementClearedSubComponent>();
		bool flag = subComponent != null && subComponent.gameObject.activeSelf;
		bool flag2 = subComponent2 != null && subComponent2.gameObject.activeSelf;
		if (flag != flag2)
		{
			if (flag)
			{
				return -1;
			}
			if (flag2)
			{
				return 1;
			}
			return 0;
		}
		if (component.HeroInfo == null || component2.HeroInfo == null)
		{
			if (component.HeroInfo != null)
			{
				return -1;
			}
			if (component2.HeroInfo != null)
			{
				return 1;
			}
		}
		return component.CreatureData.Index.CompareTo(component2.CreatureData.Index);
	}

	void AchievementComponent.Listener.OnClickRewardButton(int AchievementIndex)
	{
		if (SceneAutoBehaviour<AchievementManagement>.isValidInstance)
		{
			List<int> achievementIndexList = new List<int> { AchievementIndex };
			SceneAutoBehaviour<AchievementManagement>.instance.RequestRewardAchievements(achievementIndexList);
		}
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent comp)
	{
		if (SceneAutoBehaviour<AchievementManagement>.isValidInstance)
		{
			int heroIndex = ((comp.CreatureData != null) ? comp.CreatureData.Index : 0);
			SceneAutoBehaviour<AchievementManagement>.instance.SelectAchievementHero(heroIndex);
		}
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo heroInfo)
	{
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent heroPortrait, bool isMine)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}

	public override void ReOpen()
	{
		base.ReOpen();
		SetDirty();
	}
}
