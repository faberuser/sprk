using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class HeroDeck2 : LobbyFullWindow, LobbyTopmostMenu.IMiniGuideButtonObserver, HeroPortraitComponent.Listener, EmptyHeroPortraitComponent.Listener, IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>, IUserObserver
{
	public enum EPositionTab
	{
		Total,
		Forward,
		Middle,
		Rear,
		NotCompleted
	}

	public interface Listener
	{
		void OnClickHero(CachedHeroInfo heroInfo);

		void OnClickHero(CreatureData creatureData);

		bool OnCloseRequest(HeroDeck2 closeThis);

		void GotoHeroShop();

		void ViewDetail(int HeroIndex);

		void OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo heroInfo);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Hero/HeroDeck2";

	public UIScrollGrid PortraitGrid;

	public UIToggle ToggleTotal;

	public UIToggle ToggleForward;

	public UIToggle ToggleMiddle;

	public UIToggle ToggleRear;

	public UIToggle ToggleNotCompleted;

	public UILabel Label_Total;

	public UILabel Label_Forward;

	public UILabel Label_Middle;

	public UILabel Label_Rear;

	public UILabel Label_NotCompleted;

	public UISprite Sprite_TotalNewHero;

	public HeroPortraitComponent.PrefabSelector PortraitSelector = new HeroPortraitComponent.PrefabSelector(HeroPortraitComponent.PrefabSelector.EPrefabType.Default);

	private Listener _listener;

	private PoolingDic<int, HeroPortraitComponent> _heroDeckDic;

	private PoolingDic<int, EmptyHeroPortraitComponent> _emptyHeroDic;

	private EPositionTab _currentTab;

	public EPositionTab CurPositionTab => _currentTab;

	int LobbyTopmostMenu.IMiniGuideButtonObserver.WindowGuideIndex => WindowGuideIndex;

	public void Init(Listener listener, EPositionTab tabPos = EPositionTab.Total)
	{
		_listener = listener;
		_currentTab = tabPos;
		ToggleForward.value = tabPos == EPositionTab.Forward;
		ToggleMiddle.value = tabPos == EPositionTab.Middle;
		ToggleRear.value = tabPos == EPositionTab.Rear;
		ToggleNotCompleted.value = tabPos == EPositionTab.NotCompleted;
		ToggleTotal.value = tabPos == EPositionTab.Total;
		UIHelper.EnableGameObject(Sprite_TotalNewHero, GlobalAutoBehaviour<AccountManager>.instance.HeroManager.GetAddedHeroCount() > 0);
		SetupHeroTabLabel();
		CreateHeroItem();
		GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ClearAddedHeroList();
	}

	private void SetupHeroTabLabel()
	{
		GetHeroCount(out var Total, out var Forward, out var Middle, out var Rear, out var NotCompleted);
		string text = LocalizationManager.GetString(Hero.HERO_DECK_TOTAL);
		if (Total > 0)
		{
			text = text + " " + LocalizationManager.GetString(NShared.NLocalization.Common.PARENTHESES_NUMBER, Total);
		}
		string text2 = LocalizationManager.GetString(Hero.HERO_DECK_FORWARD);
		if (Forward > 0)
		{
			text2 = text2 + " " + LocalizationManager.GetString(NShared.NLocalization.Common.PARENTHESES_NUMBER, Forward);
		}
		string text3 = LocalizationManager.GetString(Hero.HERO_DECK_MIDDLE);
		if (Middle > 0)
		{
			text3 = text3 + " " + LocalizationManager.GetString(NShared.NLocalization.Common.PARENTHESES_NUMBER, Middle);
		}
		string text4 = LocalizationManager.GetString(Hero.HERO_DECK_REAR);
		if (Rear > 0)
		{
			text4 = text4 + " " + LocalizationManager.GetString(NShared.NLocalization.Common.PARENTHESES_NUMBER, Rear);
		}
		string text5 = LocalizationManager.GetString(Hero.HERO_DECK_NOTCOMPLETED);
		if (NotCompleted > 0)
		{
			text5 = text5 + " " + LocalizationManager.GetString(NShared.NLocalization.Common.PARENTHESES_NUMBER, NotCompleted);
		}
		UIHelper.SetLabelText(Label_Total, text);
		UIHelper.SetLabelText(Label_Forward, text2);
		UIHelper.SetLabelText(Label_Middle, text3);
		UIHelper.SetLabelText(Label_Rear, text4);
		UIHelper.SetLabelText(Label_NotCompleted, text5);
	}

	private void GetHeroCount(out int Total, out int Forward, out int Middle, out int Rear, out int NotCompleted)
	{
		Total = 0;
		Forward = 0;
		Middle = 0;
		Rear = 0;
		NotCompleted = 0;
		foreach (HeroProfileData allDatum in GameRoot.Instance.HeroProfileDataContainer.GetAllData())
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(allDatum.Index);
			if (data == null)
			{
				continue;
			}
			if (GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(data.Index) == null)
			{
				if (data.OpenType != CreatureOpenType.None && data.OpenType != CreatureOpenType.CommingSoon)
				{
					NotCompleted++;
				}
				continue;
			}
			switch (data.GetPositionType())
			{
			case CreaturePositionType.Forward:
				Forward++;
				break;
			case CreaturePositionType.Middle:
				Middle++;
				break;
			case CreaturePositionType.Rear:
				Rear++;
				break;
			}
			Total++;
		}
	}

	private void Clear()
	{
		_listener = null;
		ClearHeroItem();
	}

	public override void OnSystemBackButton()
	{
		OnClickExitButton();
	}

	public override void ReOpen()
	{
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

	public void Refresh()
	{
		UIHelper.EnableGameObject(Sprite_TotalNewHero, GlobalAutoBehaviour<AccountManager>.instance.HeroManager.GetAddedHeroCount() > 0);
		CreateHeroItem();
		GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ClearAddedHeroList();
	}

	protected virtual void CreateHeroItem()
	{
		ClearHeroItem();
		IList<HeroProfileData> allData = GameRoot.Instance.HeroProfileDataContainer.GetAllData();
		if (_currentTab == EPositionTab.NotCompleted)
		{
			CreateEmptyHeroItem(allData);
			UIHelper.RepositionGrid(PortraitGrid);
			RestoredPortal.RefreshHeroDeckDispatch(this);
			return;
		}
		List<CreatureData> list = new List<CreatureData>();
		foreach (HeroProfileData item in allData)
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(item.Index);
			if (data != null && GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(data.Index) != null)
			{
				list.Add(data);
			}
		}
		list.Sort(delegate(CreatureData l, CreatureData r)
		{
			CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(l.Index);
			CachedHeroInfo heroInfo2 = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(r.Index);
			return new HeroComparer().Compare(heroInfo, heroInfo2);
		});
		list.ForEach(delegate(CreatureData heroData)
		{
			if (_currentTab != EPositionTab.Total && _currentTab != EPositionTab.NotCompleted)
			{
				CreaturePositionType positionType = heroData.GetPositionType();
				if ((_currentTab == EPositionTab.Forward && positionType != CreaturePositionType.Forward) || (_currentTab == EPositionTab.Middle && positionType != CreaturePositionType.Middle) || (_currentTab == EPositionTab.Rear && positionType != CreaturePositionType.Rear))
				{
					return;
				}
			}
			CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroData.Index);
			CreateHeroItem(heroData, heroInfo);
		});
		UIHelper.RepositionGrid(PortraitGrid);
		RestoredPortal.RefreshHeroDeckDispatch(this);
	}

	private void CreateEmptyHeroItem(IList<HeroProfileData> heroProfileDatas)
	{
		foreach (HeroProfileData heroProfileData in heroProfileDatas)
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroProfileData.Index);
			if (data != null && data.OpenType != CreatureOpenType.None && data.OpenType != CreatureOpenType.CommingSoon && GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(data.Index) == null)
			{
				CreateEmptyHeroItem(data);
			}
		}
	}

	public override void ReserveCache(UiPreloader preloader)
	{
		base.ReserveCache(preloader);
	}

	protected void ClearHeroItem()
	{
		FactoryBase.ReleaseAndClearList(ref _heroDeckDic);
		FactoryBase.ReleaseAndClearList(ref _emptyHeroDic);
	}

	protected void CreateHeroItem(CreatureData creatureData, CachedHeroInfo cachedHeroInfo)
	{
		if (cachedHeroInfo != null)
		{
			HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(PortraitGrid, PortraitSelector.PrefabAuto);
			heroPortraitComponent.Open(cachedHeroInfo, this);
			heroPortraitComponent.ShowCreatureName();
			heroPortraitComponent.SetNew(GlobalAutoBehaviour<AccountManager>.instance.HeroManager.InAddedHeroList(creatureData.Index));
			_heroDeckDic.Add(creatureData.Index, heroPortraitComponent);
		}
	}

	private void RefreshHeroNotify()
	{
		foreach (HeroPortraitComponent value in _heroDeckDic.Values)
		{
			List<HeroNotifyComponent.ENotifyInfo> heroNotifyInfoList = SceneAutoBehaviour<HeroNotifyContainer>.instance.GetHeroNotifyInfoList(value.HeroInfo.HeroIndex);
			value.AddHeroNotify(heroNotifyInfoList);
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		RefreshHeroNotify();
	}

	private void CreateEmptyHeroItem(CreatureData creatureData)
	{
		EmptyHeroPortraitComponent emptyHeroPortraitComponent = UIHelper.AddGridItem(PortraitGrid, EmptyHeroPortraitComponent.PrefabAuto);
		emptyHeroPortraitComponent.Open(creatureData, this);
		_emptyHeroDic.Add(creatureData.Index, emptyHeroPortraitComponent);
	}

	public void OnClickExitButton()
	{
		if (_listener == null || _listener.OnCloseRequest(this))
		{
			Close();
		}
	}

	public void OnToggleChange()
	{
		EPositionTab ePositionTab = EPositionTab.Total;
		if (ToggleForward.value)
		{
			ePositionTab = EPositionTab.Forward;
		}
		if (ToggleMiddle.value)
		{
			ePositionTab = EPositionTab.Middle;
		}
		if (ToggleRear.value)
		{
			ePositionTab = EPositionTab.Rear;
		}
		if (ToggleNotCompleted.value)
		{
			ePositionTab = EPositionTab.NotCompleted;
		}
		if (ePositionTab == EPositionTab.NotCompleted && UIEventAttribute.NeedCheck())
		{
			GetToggle(_currentTab).value = true;
			UIEventAttribute.ShowMessage();
		}
		else if (ePositionTab != _currentTab)
		{
			_currentTab = ePositionTab;
			UIHelper.EnableGameObject(Sprite_TotalNewHero, enable: false);
			CreateHeroItem();
		}
	}

	private UIToggle GetToggle(EPositionTab tab)
	{
		return tab switch
		{
			EPositionTab.Total => ToggleTotal, 
			EPositionTab.Forward => ToggleForward, 
			EPositionTab.Middle => ToggleMiddle, 
			EPositionTab.Rear => ToggleRear, 
			EPositionTab.NotCompleted => ToggleNotCompleted, 
			_ => null, 
		};
	}

	public void OnClickGotoHeroShop()
	{
		if (_listener != null)
		{
			_listener.GotoHeroShop();
		}
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
		if (heroPortrait.HeroInfo != null)
		{
			if (_listener != null)
			{
				_listener.OnClickHero(heroPortrait.HeroInfo);
			}
		}
		else if (_listener != null)
		{
			_listener.OnClickHero(heroPortrait.CreatureData);
		}
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent heroPortrait, bool isMine)
	{
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo cachedHeroInfo)
	{
		if (_listener != null && cachedHeroInfo != null)
		{
			_listener.OnClickHero(cachedHeroInfo);
		}
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}

	void EmptyHeroPortraitComponent.Listener.ViewDetail(int HeroIndex)
	{
		if (_listener != null)
		{
			_listener.ViewDetail(HeroIndex);
		}
	}

	public static EPositionTab GetPositionTabType(CreaturePositionType positionType)
	{
		return positionType switch
		{
			CreaturePositionType.Forward => EPositionTab.Forward, 
			CreaturePositionType.Middle => EPositionTab.Middle, 
			CreaturePositionType.Rear => EPositionTab.Rear, 
			_ => EPositionTab.Total, 
		};
	}

	void IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>.OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changedInfo)
	{
		HeroPortraitComponent vALUE = _heroDeckDic.GETVALUE(changedInfo.HeroIndex, null);
		if (!(vALUE == null))
		{
			if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.HeroLevelup)
			{
				vALUE.SetLevel(show: true);
				CreateHeroItem();
			}
			if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.Costume)
			{
				vALUE.SetHeroImage();
			}
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	public void TutorialInit(Listener listener, EPositionTab tabPos = EPositionTab.Total)
	{
		_listener = listener;
		_currentTab = tabPos;
		ToggleForward.value = tabPos == EPositionTab.Forward;
		ToggleMiddle.value = tabPos == EPositionTab.Middle;
		ToggleRear.value = tabPos == EPositionTab.Rear;
		ToggleNotCompleted.value = tabPos == EPositionTab.NotCompleted;
		ToggleTotal.value = tabPos == EPositionTab.Total;
		UIHelper.EnableGameObject(Sprite_TotalNewHero, enable: false);
		SetupHeroTabLabel();
		TutorialCreateHeroItem();
		GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ClearAddedHeroList();
	}

	private void TutorialCreateHeroItem()
	{
		ClearHeroItem();
		IList<HeroProfileData> allData = GameRoot.Instance.HeroProfileDataContainer.GetAllData();
		if (_currentTab == EPositionTab.NotCompleted)
		{
			CreateEmptyHeroItem(allData);
			UIHelper.RepositionGrid(PortraitGrid);
			return;
		}
		List<CreatureData> list = new List<CreatureData>();
		foreach (HeroProfileData item in allData)
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(item.Index);
			if (data != null && GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(data.Index) != null)
			{
				list.Add(data);
			}
		}
		list.Sort((CreatureData l, CreatureData r) => l.Index.CompareTo(r.Index));
		list.ForEach(delegate(CreatureData heroData)
		{
			if (_currentTab != EPositionTab.Total && _currentTab != EPositionTab.NotCompleted)
			{
				CreaturePositionType positionType = heroData.GetPositionType();
				if ((_currentTab == EPositionTab.Forward && positionType != CreaturePositionType.Forward) || (_currentTab == EPositionTab.Middle && positionType != CreaturePositionType.Middle) || (_currentTab == EPositionTab.Rear && positionType != CreaturePositionType.Rear))
				{
					return;
				}
			}
			CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroData.Index);
			CreateHeroItem(heroData, heroInfo);
		});
		UIHelper.RepositionGrid(PortraitGrid);
	}
}
