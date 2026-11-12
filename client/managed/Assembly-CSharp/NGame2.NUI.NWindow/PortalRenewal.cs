using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NLobby;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NPayShop;
using NGame2.NUI.NHelper.NPortal;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.NChatting;
using NGame2.NUI.NWindow.NPayShop;
using NGame2.NUI.NWindow.NPortal;
using NGame2.NUtil;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class PortalRenewal : LobbyFullWindow, ContentsMenuComponent.Listener, PortalContentsComponent.Listener, PayShopResourceList.IListener, IUserObserver<UserManager.UserStatInfo>, IUserObserver, IUserObserver<NGame2.NAccount.UserPlayerCurrencyManager.ChangedCurrencyInfo>, IUserObserver<ItemInfo>
{
	[Serializable]
	public class ContentsGridData
	{
		public PortalRenewalCompType portalRenewalCompType;

		public UIGrid CompGrid;
	}

	public enum MenuEditMode
	{
		IDLE,
		RUN
	}

	public enum LockedState
	{
		NONE,
		CONDITION_LOCKED,
		TUTORIAL_LOCKED
	}

	public enum NoticeState
	{
		NONE,
		UNCONFIRMED,
		NEW,
		UNCONFIRMED_AND_NEW
	}

	[Serializable]
	public class EditButton
	{
		public UIButton UIButton;

		public Transform DiableTransform;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Portal/PortalRenewal";

	[Header("[Menu Group]")]
	public UILabel MenuTitleLabel;

	public UIGrid PinMenuGrid;

	public UIScrollView MenuScrollView;

	public UIGrid MenuGrid;

	public Transform PinedMenuVignettingTranform;

	public TweenAlpha VignettingTweenAlpha;

	public Transform EditControlerRoot;

	public UILabel EditButtonLabel;

	public EditButton EditPrevButton;

	public EditButton EditNextButton;

	[Header("[Contents Group]")]
	public UILabel ContentsTitleLabel;

	public UIScrollView ContentsScrollView;

	public List<ContentsGridData> ContentsGridDatas;

	public Transform ContentsSingleRootTranform;

	[Header("[Effect Group]")]
	public UIGame3DWidget OpeningEffectWidget;

	public string OpeningEffectCode;

	public Vector3 OpeningEffectPos;

	public float OpeningEffectScale;

	public string OpeningSoundCode;

	[Header("[Manage Depth Group]")]
	public UIPanel TopPanel;

	[Header("[Openning Effect Controller]")]
	public float MenuRotDelay;

	public float MenuRotMultiple;

	public List<UITweener> TweenList;

	[Header("[Other Script]")]
	public PayShopResourceList PayShopResourceList;

	private List<PortalRenewalData> _categoryDatas;

	private PortalRenewalCategory _currCategory;

	private MenuEditMode _menuEditMode;

	private List<ContentsMenuComponent> _contentsMenuComponents = new List<ContentsMenuComponent>();

	private List<PortalContentsComponent> _contentsComponents = new List<PortalContentsComponent>();

	private PortalRenewalData _currMenuData;

	private ContentsMenuComponent _currSelectMenuComp;

	private ContentsMenuComponent _currEditMenuComp;

	private PortalMenuController _portalMenuController;

	private int _originalChapterIndex;

	private int _originalDungeonIndex;

	private List<Coroutine> _coroutineList = new List<Coroutine>();

	private ContentsMenuComponent PossibleEditFirstContentsMenuComponent
	{
		get
		{
			if (_currSelectMenuComp != null && _currSelectMenuComp._portalRenewalData != null && !_currSelectMenuComp._portalRenewalData.Pined)
			{
				return _currSelectMenuComp;
			}
			if (__Extension_Method_ICollection__.IsNullOrEmpty(_contentsMenuComponents))
			{
				return null;
			}
			ContentsMenuComponent contentsMenuComponent = null;
			for (int i = 0; i < _contentsMenuComponents.Count; i++)
			{
				contentsMenuComponent = _contentsMenuComponents[i];
				if (!(contentsMenuComponent == null) && contentsMenuComponent._portalRenewalData != null && !contentsMenuComponent._portalRenewalData.Pined)
				{
					return contentsMenuComponent;
				}
			}
			return null;
		}
	}

	private PortalMenuController PortalMenuController
	{
		get
		{
			if (_portalMenuController == null)
			{
				_portalMenuController = new PortalMenuController();
			}
			return _portalMenuController;
		}
	}

	private List<PortalRenewalData> PortalRenewalDataList => PortalMenuController.PortalRenewalDataList;

	public void Open(PortalRenewalCategory focusCategory)
	{
		Open();
		_currCategory = PortalRenewalCategory.NONE;
		if (!InitMenu())
		{
			base.Close();
		}
		else
		{
			_coroutineList.Add(VespaCoroutine.Start(LateOpenContents(focusCategory)));
		}
	}

	private IEnumerator LateOpenContents(PortalRenewalCategory focusCategory)
	{
		yield return null;
		ContentsMenuComponent menuComp = _contentsMenuComponents.Find((ContentsMenuComponent x) => x._portalRenewalData.Category == focusCategory);
		SetContents(focusCategory, menuComp);
		SetEditMode(MenuEditMode.IDLE);
		GlobalAutoBehaviour<AccountManager>.instance.ItemManager.ObserverManager.AddObserver(this);
		_originalChapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		_originalDungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		TransformData transformData = new TransformData();
		transformData.Position = OpeningEffectPos;
		transformData.Scale = OpeningEffectScale;
		SoundStock stock = new SoundStock
		{
			Code = OpeningSoundCode,
			LocalTimer = MainTimer.DefaultLocal
		};
		SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
		ContentsScrollView.panel.ForceUpdateAnchors(includeChildren: false);
		OpeningEffectWidget.PlayInstantEffect(OpeningEffectCode, transformData, null);
		foreach (UITweener tween in TweenList)
		{
			tween.enabled = false;
			if ((bool)(tween as TweenAlpha))
			{
				TweenAlpha tweenAlpha = (TweenAlpha)tween;
				UIWidget component = tween.GetComponent<UIWidget>();
				if (component != null)
				{
					component.alpha = tweenAlpha.from;
				}
			}
			else if ((bool)(tween as TweenPosition))
			{
				TweenPosition tweenPosition = (TweenPosition)tween;
				tweenPosition.transform.localPosition = tweenPosition.from;
			}
		}
		yield return null;
		TweenList.ForEach(delegate(UITweener tween)
		{
			tween.PlayForwardReset();
		});
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		_contentsComponents.ForEach(delegate(PortalContentsComponent x)
		{
			x.UpdateComponent();
		});
	}

	public override void Close()
	{
		FactoryBase.ReleaseAndClearList(_contentsMenuComponents);
		FactoryBase.ReleaseAndClearList(_contentsComponents);
		_currCategory = PortalRenewalCategory.NONE;
		_menuEditMode = MenuEditMode.IDLE;
		_currMenuData = null;
		_currSelectMenuComp = null;
		_currEditMenuComp = null;
		PayShopResourceList.ClearAll();
		if (PortalMenuController != null)
		{
			PortalMenuController.Close();
			_portalMenuController = null;
		}
		GlobalAutoBehaviour<AccountManager>.instance.ItemManager.ObserverManager.RemoveObserver(this);
		NGame2.NAccount.UserCampaignManager.ChapterIndex = _originalChapterIndex;
		NGame2.NAccount.UserCampaignManager.DungeonIndex = _originalDungeonIndex;
		PayShopMessage3ButtonPopup.ClosePortalMovePopup();
		VespaCoroutine.Stop(this, _coroutineList);
		_coroutineList.Clear();
		base.Close();
	}

	public void OnClickEditMenuList()
	{
		switch (_menuEditMode)
		{
		case MenuEditMode.IDLE:
			_currEditMenuComp = PossibleEditFirstContentsMenuComponent;
			SetEditMode(MenuEditMode.RUN);
			EnableCurrentSelectMenuHighLight(MenuEditMode.IDLE, enable: false);
			EnableCurrentEditMenuHighLight(_menuEditMode, enable: true);
			break;
		case MenuEditMode.RUN:
			SetEditMode(MenuEditMode.IDLE);
			EnableCurrentEditMenuHighLight(MenuEditMode.RUN, enable: false);
			EnableCurrentSelectMenuHighLight(_menuEditMode, enable: true);
			break;
		}
		RefreshEditButtonState();
		if (PortalMenuController != null)
		{
			PortalMenuController.SavePortalMenuCategory();
		}
		EnableVignetting();
	}

	private void SetEditMode(MenuEditMode menuEditMode)
	{
		UIHelper.EnableGameObject(EditControlerRoot, menuEditMode == MenuEditMode.RUN);
		_menuEditMode = menuEditMode;
		SetEditButtonLabel();
	}

	private void SetEditButtonLabel()
	{
		UIHelper.SetLabelText(EditButtonLabel, LocalizationManager.GetStringDirect((_menuEditMode == MenuEditMode.IDLE) ? "PORTAL_UI_TEXT_PREFERENCE_EDIT_LIST" : "PORTAL_UI_TEXT_PREFERENCE_EDIT_LIST_COMPLETE"));
	}

	public void OnClickEditPrevButton()
	{
		if (_menuEditMode == MenuEditMode.RUN && !(_currEditMenuComp == null))
		{
			SwapPortalRenewalCategory(-1);
			RefreshEditButtonState();
		}
	}

	public void OnClickEditNextButton()
	{
		if (_menuEditMode == MenuEditMode.RUN && !(_currEditMenuComp == null))
		{
			SwapPortalRenewalCategory(1);
			RefreshEditButtonState();
		}
	}

	private void SwapPortalRenewalCategory(int direction)
	{
		if (_currEditMenuComp == null || _currEditMenuComp._portalRenewalData == null)
		{
			return;
		}
		int num = FindIndex(_currEditMenuComp._portalRenewalData);
		if (num < 0)
		{
			return;
		}
		PortalRenewalData toSwapData = null;
		PortalRenewalData fromSwapData = null;
		int swapIndex = -1;
		if (PortalMenuController.SwapPortalRenewalCategory(num, direction, out toSwapData, out fromSwapData, out swapIndex))
		{
			ContentsMenuComponent contentsMenuComponent = _contentsMenuComponents[swapIndex];
			if (!(contentsMenuComponent == null))
			{
				NoticeState noticeState = _currEditMenuComp.NoticeState;
				NoticeState noticeState2 = contentsMenuComponent.NoticeState;
				_currEditMenuComp.Open(fromSwapData, this);
				contentsMenuComponent.Open(toSwapData, this);
				_currEditMenuComp.ChangeNoticeState(noticeState2);
				contentsMenuComponent.ChangeNoticeState(noticeState);
				_currEditMenuComp = contentsMenuComponent;
				EnableCurrentEditMenuHighLight(_menuEditMode, enable: true);
				SetCurrentSelectMenuComponent();
			}
		}
	}

	private void SetCurrentSelectMenuComponent()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_contentsMenuComponents) && _currCategory != PortalRenewalCategory.NONE)
		{
			_currSelectMenuComp = _contentsMenuComponents.Find((ContentsMenuComponent comp) => comp.Category == _currCategory);
		}
	}

	private bool InitMenu()
	{
		FactoryBase.ReleaseAndClearList(_contentsMenuComponents);
		_categoryDatas = PortalRenewalDataList;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_categoryDatas))
		{
			return false;
		}
		float num = 0f;
		foreach (PortalRenewalData categoryData in _categoryDatas)
		{
			if (categoryData.Category != PortalRenewalCategory.NONE && SingletonAuto<ConditionChecker>.instance.Check(categoryData.ShowCondition))
			{
				UIGrid uIGrid = (categoryData.Pined ? PinMenuGrid : MenuGrid);
				ContentsMenuComponent contentsMenuComponent = null;
				contentsMenuComponent = (categoryData.Pined ? ContentsMenuComponent.PrefabAuto2.Factory.Create(uIGrid.transform) : ContentsMenuComponent.PrefabAuto.Factory.Create(uIGrid.transform));
				if (!(contentsMenuComponent == null))
				{
					contentsMenuComponent.Open(categoryData, this, MenuRotDelay + MenuRotMultiple * num);
					num += 1f;
					_contentsMenuComponents.Add(contentsMenuComponent);
				}
			}
		}
		MenuScrollView.ResetPosition();
		PayShopResourceList.Open(this);
		EnableVignetting();
		return true;
	}

	private void EnableVignetting()
	{
		UIHelper.EnableGameObject(PinedMenuVignettingTranform, _menuEditMode == MenuEditMode.RUN);
		if (!UIHelper.IsEnabledGameObjet(PinedMenuVignettingTranform))
		{
			VignettingTweenAlpha.PlayForwardReset();
			return;
		}
		VignettingTweenAlpha.PlayForward();
		if (!(PinedMenuVignettingTranform == null))
		{
			UIPanel component = PinedMenuVignettingTranform.GetComponent<UIPanel>();
			if (!(component == null))
			{
				FixPanelDepth();
				component.depth = TopPanel.depth + 1;
				TopPanel.depth = component.depth + 1;
			}
		}
	}

	private void RefreshEditButtonState()
	{
		EnableEditButton(EditPrevButton, enable: false);
		EnableEditButton(EditNextButton, enable: false);
		if (!(_currEditMenuComp == null))
		{
			int num = FindIndex(_currEditMenuComp._portalRenewalData);
			if (num >= 0 && PortalMenuController != null)
			{
				EnableEditButton(EditPrevButton, PortalMenuController.CheckPossibleSwap(num, -1));
				EnableEditButton(EditNextButton, PortalMenuController.CheckPossibleSwap(num, 1));
			}
		}
	}

	private void EnableEditButton(EditButton button, bool enable)
	{
		if (button != null)
		{
			UIHelper.EnableButton(button.UIButton, enable);
			UIHelper.EnableGameObject(button.DiableTransform, !enable);
		}
	}

	private void SetContents(PortalRenewalCategory category, ContentsMenuComponent menuComp)
	{
		if (category == PortalRenewalCategory.NONE || category == _currCategory || _categoryDatas.Find((PortalRenewalData x) => x.Category == category) == null)
		{
			return;
		}
		if (_currSelectMenuComp != null)
		{
			if (_currSelectMenuComp.Category == PortalRenewalCategory.Guild || _currSelectMenuComp.Category == PortalRenewalCategory.Arena)
			{
				switch (_currSelectMenuComp.NoticeState)
				{
				case NoticeState.NEW:
					_currSelectMenuComp.ChangeNoticeState(NoticeState.NONE);
					break;
				case NoticeState.UNCONFIRMED_AND_NEW:
					_currSelectMenuComp.ChangeNoticeState(NoticeState.UNCONFIRMED);
					break;
				}
			}
			else
			{
				_currSelectMenuComp.CheckChildNoticeState();
			}
		}
		EnableCurrentSelectMenuHighLight(_menuEditMode, enable: false);
		_currSelectMenuComp = menuComp;
		EnableCurrentSelectMenuHighLight(_menuEditMode, enable: true);
		FactoryBase.ReleaseAndClearList(_contentsComponents);
		_currCategory = category;
		RefreshContentsUI();
	}

	private void RefreshContentsUI()
	{
		List<PortalRenewalData> list = GameRoot.Instance.PortalRenewalDataContainer.GetData(_currCategory).ToList();
		PortalRenewalData portalRenewalData = _categoryDatas.Find((PortalRenewalData x) => x.Category == _currCategory);
		if (portalRenewalData == null)
		{
			return;
		}
		_currMenuData = portalRenewalData;
		UIHelper.EnableGameObject(ContentsTitleLabel, _currMenuData.SubComponentShape != PortalRenewalCompType.Window);
		UIHelper.SetLabelText(ContentsTitleLabel, LocalizationManager.GetStringDirect(_currMenuData.TitleLocalKey));
		PortalRenewalData portalRenewalData2 = list.Find((PortalRenewalData x) => x.SubCategory == PortalRenewalSubCategory.MENUBUTTON);
		PortalRenewalCompType subComponentShape = portalRenewalData2.SubComponentShape;
		UIGrid componentGrid = GetComponentGrid(subComponentShape);
		if (portalRenewalData2.Category == PortalRenewalCategory.Chapter)
		{
			PortalRenewalData portalRenewalData3 = list.Find((PortalRenewalData x) => x.SubCategory == PortalRenewalSubCategory.Chapter);
			GenerateComponent(portalRenewalData3, subComponentShape, (componentGrid == null) ? null : componentGrid);
		}
		else if (RestoredPortal.UseShakmehWindow(portalRenewalData2))
		{
			PortalRenewalData portalRenewalData4 = list.Find((PortalRenewalData x) => x.SubCategory == PortalRenewalSubCategory.Shakmeh);
			GenerateComponent(portalRenewalData4, subComponentShape, (componentGrid == null) ? null : componentGrid);
		}
		else if (portalRenewalData2.Category == PortalRenewalCategory.Pet)
		{
			PortalRenewalData portalRenewalData5 = list.Find((PortalRenewalData x) => x.SubCategory == PortalRenewalSubCategory.Pet);
			GenerateComponent(portalRenewalData5, subComponentShape, (componentGrid == null) ? null : componentGrid);
		}
		else
		{
			list.Sort((PortalRenewalData l, PortalRenewalData r) => l.ContentsShowIndex.CompareTo(r.ContentsShowIndex));
			foreach (PortalRenewalData item in list)
			{
				if (item != null && item.SubCategory != PortalRenewalSubCategory.MENUBUTTON && SingletonAuto<ConditionChecker>.instance.Check(item.ShowCondition))
				{
					GenerateComponent(item, subComponentShape, (componentGrid == null) ? null : componentGrid);
				}
			}
		}
		FixPanelDepth();
		if (componentGrid != null)
		{
			componentGrid.Reposition();
		}
		Vector2 vector = new Vector2
		{
			x = ContentsScrollView.transform.localPosition.x + ContentsScrollView.panel.baseClipRegion.x,
			y = ContentsScrollView.transform.localPosition.y + ContentsScrollView.panel.baseClipRegion.y
		};
		ContentsScrollView.transform.localPosition = vector;
		ContentsScrollView.ResetPosition();
	}

	public void RefreshUI_GuildExist()
	{
		SetEditMode(MenuEditMode.IDLE);
		InitMenu();
		PortalRenewalCategory currCategory = _currCategory;
		if ((uint)(currCategory - 1) > 1u && currCategory != PortalRenewalCategory.Guild && (uint)(currCategory - 17) > 1u)
		{
			return;
		}
		EnableCurrentSelectMenuHighLight(_menuEditMode, enable: false);
		PortalRenewalCategory portalRenewalCategory = _currCategory;
		ContentsMenuComponent menuComp = _currSelectMenuComp;
		if (portalRenewalCategory == PortalRenewalCategory.GuildTerritory || portalRenewalCategory == PortalRenewalCategory.Guild)
		{
			menuComp = _contentsMenuComponents.Find((ContentsMenuComponent x) => x._portalRenewalData.Category == PortalRenewalCategory.Chapter);
			portalRenewalCategory = PortalRenewalCategory.Chapter;
		}
		_currCategory = PortalRenewalCategory.NONE;
		SetContents(portalRenewalCategory, menuComp);
		PayShopMessage3ButtonPopup.ClosePortalMovePopup();
	}

	private void GenerateComponent(PortalRenewalData portalRenewalData, PortalRenewalCompType compType, UIGrid grid)
	{
		PortalContentsComponent portalContentsComponent = null;
		switch (compType)
		{
		case PortalRenewalCompType.Small:
			portalContentsComponent = PortalSmallComponent.PrefabAuto.Factory.Create(grid.transform);
			break;
		case PortalRenewalCompType.Half:
			portalContentsComponent = PortalHalfComponent.PrefabAuto.Factory.Create(grid.transform);
			break;
		case PortalRenewalCompType.Third:
			portalContentsComponent = PortalThirdComponent.PrefabAuto.Factory.Create(grid.transform);
			break;
		case PortalRenewalCompType.Quarter:
			portalContentsComponent = PortalQuaterComponent.PrefabAuto.Factory.Create(grid.transform);
			break;
		case PortalRenewalCompType.Window:
			portalContentsComponent = GetPortalWindowComponent(portalRenewalData.Category);
			break;
		}
		if (portalContentsComponent != null)
		{
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(portalContentsComponent.transform);
			portalContentsComponent.Open(portalRenewalData, this);
			portalContentsComponent.FixPanelsDepth(100);
			ContentsScrollView.panel.UpdateAnchors();
			float num = ContentsScrollView.panel.width;
			if (compType == PortalRenewalCompType.Small)
			{
				num /= 2f;
			}
			portalContentsComponent.ResolutionComp(num);
			if (grid != null)
			{
				grid.cellWidth = num;
			}
			_contentsComponents.Add(portalContentsComponent);
		}
	}

	private void EnableCurrentSelectMenuHighLight(MenuEditMode editMode, bool enable)
	{
		if (_currSelectMenuComp != null)
		{
			_currSelectMenuComp.SetSelectedHighLight(editMode, enable);
		}
	}

	private void EnableCurrentEditMenuHighLight(MenuEditMode editMode, bool enable)
	{
		if (_currEditMenuComp != null)
		{
			_currEditMenuComp.SetSelectedHighLight(editMode, enable);
		}
	}

	private UIGrid GetComponentGrid(PortalRenewalCompType compType)
	{
		return ContentsGridDatas.Find((ContentsGridData x) => x.portalRenewalCompType == compType)?.CompGrid;
	}

	private PortalContentsComponent GetPortalWindowComponent(PortalRenewalCategory category)
	{
		if (ContentsSingleRootTranform == null)
		{
			return null;
		}
		return category switch
		{
			PortalRenewalCategory.Chapter => PortalChapterComponent.PrefabAuto.Factory.Create(ContentsSingleRootTranform), 
			PortalRenewalCategory.ChallengeRaid => PortalChallengeRaidComponent.PrefabAuto.Factory.Create(ContentsSingleRootTranform), 
			PortalRenewalCategory.WorldBoss => PortalWorldBossComponent.PrefabAuto.Factory.Create(ContentsSingleRootTranform), 
			PortalRenewalCategory.Eclipse => PortalEclipseComponent.PrefabAuto.Factory.Create(ContentsSingleRootTranform), 
			PortalRenewalCategory.Shakmeh => PortalShakmehComponent.PrefabAuto.Factory.Create(ContentsSingleRootTranform), 
			PortalRenewalCategory.Pet => PortalPethouseComponent.PrefabAuto.Factory.Create(ContentsSingleRootTranform), 
			PortalRenewalCategory.TreasureHouse => PortalTreasureHouseComponent.PrefabAuto.Factory.Create(ContentsSingleRootTranform), 
			PortalRenewalCategory.PunishmentRaid => PortalPunishmentRaidComponent.PrefabAuto.Factory.Create(ContentsSingleRootTranform), 
			PortalRenewalCategory.PunishmentRuneCraft => PortalPunishmentRuneCraftComponent.PrefabAuto.Factory.Create(ContentsSingleRootTranform), 
			_ => null, 
		};
	}

	private int FindIndex(PortalRenewalData compareData)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_contentsMenuComponents))
		{
			return -1;
		}
		PortalRenewalData portalRenewalData = null;
		for (int i = 0; i < _contentsMenuComponents.Count; i++)
		{
			if (!(_contentsMenuComponents[i] == null))
			{
				portalRenewalData = _contentsMenuComponents[i]._portalRenewalData;
				if (portalRenewalData != null && portalRenewalData.Category == compareData.Category && portalRenewalData.SubCategory == compareData.SubCategory)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public void OnclickMail()
	{
		if (!IsEditMode())
		{
			if (!GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpened<MailListPopup>())
			{
				SceneAutoBehaviour<MailManagement>.instance.Open();
			}
			else
			{
				SceneAutoBehaviour<MailManagement>.instance.CloseAllActiveWindow();
			}
		}
	}

	public void OnClickChat()
	{
		if (IsEditMode())
		{
			return;
		}
		string backButtonTitle;
		ETopmostMenuType topmostMenuType = GlobalAutoBehaviour<GlobalManagement>.instance.GetTopmostMenuType(out backButtonTitle);
		if (topmostMenuType == ETopmostMenuType.Tutorial || topmostMenuType == ETopmostMenuType.Tutorial_Guide)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("TUTORIAL_RAIDPARTYSETTING_3"));
			return;
		}
		SceneAutoBehaviour<MailManagement>.instance.CloseAllActiveWindow();
		SceneAutoBehaviour<PayShopManagement>.instance.CloseAllActiveWindow();
		if (!GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpened<ChattingWindow>())
		{
			SceneAutoBehaviour<ChattingManagement>.instance.OpenChatting(ChatType.None, 0L);
		}
		else
		{
			SceneAutoBehaviour<ChattingManagement>.instance.CloseAllActiveWindow();
		}
	}

	public void OnClickRevertEditContetentsMenu()
	{
		if (!IsEditMode(showCenterMsg: false) || PortalMenuController == null || !PortalMenuController.CheckRevertPortalContentsMenu())
		{
			return;
		}
		PortalMenuController.InitDefualtSortPortalMenu();
		if (InitMenu())
		{
			SetCurrentSelectMenuComponent();
			_currEditMenuComp = PossibleEditFirstContentsMenuComponent;
			EnableCurrentEditMenuHighLight(_menuEditMode, enable: true);
			RefreshEditButtonState();
			_contentsMenuComponents.ForEach(delegate(ContentsMenuComponent menu)
			{
				menu.CheckChildNoticeState(isForce: true);
			});
		}
	}

	private bool IsEditMode(bool showCenterMsg = true)
	{
		bool num = _menuEditMode == MenuEditMode.RUN;
		if (num && showCenterMsg)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("RESULT_TYPE.PORTAL_LISTEDIT_CANNOT_MOVE"));
		}
		return num;
	}

	private bool IsMoveActivityType(ClientActivityType activityType)
	{
		if (activityType == ClientActivityType.MoveToNode || activityType == ClientActivityType.MoveToNpc || activityType == ClientActivityType.MoveToLastDungeon || activityType == ClientActivityType.MoveToLastHighLevelDungeon || activityType == ClientActivityType.MoveToLastConquestDungeon)
		{
			return RestoredPortal.ClosePortalForActivity(original: true, activityType);
		}
		return RestoredPortal.ClosePortalForActivity(original: false, activityType);
	}

	private void FixPanelDepth()
	{
		MakeDirty();
		int num = 0;
		UIPanel[] componentsInChildren = GetComponentsInChildren<UIPanel>();
		foreach (UIPanel uIPanel in componentsInChildren)
		{
			num = Mathf.Max(num, uIPanel.depth);
		}
		TopPanel.depth = num + 1;
	}

	public ContentsMenuComponent GetMenuCompByCategory(PortalRenewalCategory category)
	{
		if (category == PortalRenewalCategory.NONE)
		{
			return null;
		}
		return _contentsMenuComponents.Find((ContentsMenuComponent x) => x._portalRenewalData.Category == category);
	}

	public PortalContentsComponent GetContentsComponentBySubCategory(PortalRenewalSubCategory subCategory)
	{
		return _contentsComponents.Find((PortalContentsComponent x) => x._portalRenewalData.SubCategory == subCategory);
	}

	public PortalContentsComponent GetContentsComponentByPortalData(PortalRenewalData data)
	{
		if (data == null)
		{
			return null;
		}
		return _contentsComponents.Find((PortalContentsComponent x) => x._portalRenewalData.Category == data.Category && x._portalRenewalData.SubCategory == data.SubCategory);
	}

	void ContentsMenuComponent.Listener.OnClickCategoryButton(ContentsMenuComponent component, PortalRenewalData portalRenewalData)
	{
		if (_menuEditMode == MenuEditMode.IDLE)
		{
			if (component.LockedState == LockedState.CONDITION_LOCKED)
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect(portalRenewalData.OpenConditionLocalKey));
			}
			else if (component.LockedState == LockedState.TUTORIAL_LOCKED)
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("TUTORIAL_DISABLE_DES"));
			}
			else if (portalRenewalData.MoveTaskType > ClientActivityType.None)
			{
				string taskType = portalRenewalData.MoveTaskType.ToString();
				string[] moveTaskValue = portalRenewalData.MoveTaskValue;
				SceneAutoBehaviour<ActivityManager>.instance.Run(taskType, moveTaskValue);
				if (portalRenewalData.Category != PortalRenewalCategory.WorldMap)
				{
					Close();
				}
			}
			else
			{
				SetContents(portalRenewalData.Category, component);
			}
		}
		else if (_menuEditMode == MenuEditMode.RUN && !(component == null))
		{
			if (portalRenewalData.Pined)
			{
				IsEditMode();
				return;
			}
			EnableCurrentEditMenuHighLight(_menuEditMode, enable: false);
			_currEditMenuComp = component;
			EnableCurrentEditMenuHighLight(_menuEditMode, enable: true);
			RefreshEditButtonState();
		}
	}

	bool ContentsMenuComponent.Listener.IsEditMode()
	{
		return IsEditMode(showCenterMsg: false);
	}

	void PortalContentsComponent.Listener.OnClickContentsButton(PortalRenewalData portalRenewalData)
	{
		if (IsEditMode())
		{
			return;
		}
		if (portalRenewalData.TutorialIndex > 0 && GlobalAutoBehaviour<AccountManager>.instance.TutorialManager != null && !SprkAdventureUnlocks.PortalTutorial(GlobalAutoBehaviour<AccountManager>.instance.TutorialManager, portalRenewalData.TutorialIndex))
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("TUTORIAL_DISABLE_DES"));
		}
		else
		{
			if (portalRenewalData.MoveTaskType <= ClientActivityType.None)
			{
				return;
			}
			string text = PortalHelper.GetExceptionTitle(portalRenewalData);
			if (String_Extension_Method.IsNullOrEmpty(text))
			{
				text = LocalizationManager.GetStringDirect(portalRenewalData.TitleLocalKey);
			}
			PortalHelper.OpenContentsMovePopup(text, delegate
			{
				ClientActivityType moveTaskType = portalRenewalData.MoveTaskType;
				if (IsMoveActivityType(moveTaskType))
				{
					Close();
				}
				SceneAutoBehaviour<ActivityManager>.instance.Run(portalRenewalData.MoveTaskType.ToString(), portalRenewalData.MoveTaskValue);
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(portalRenewalData.AdventureNoticeMove) && __Extension_Method_ICollection__.COUNT(portalRenewalData.AdventureNoticeMove) == 2)
				{
					int chapterIndex = portalRenewalData.AdventureNoticeMove[0];
					int dungeonIndex = portalRenewalData.AdventureNoticeMove[1];
					NGame2.NAccount.UserCampaignManager.ChapterIndex = chapterIndex;
					NGame2.NAccount.UserCampaignManager.DungeonIndex = dungeonIndex;
				}
			});
		}
	}

	void PortalContentsComponent.Listener.ChangeContentsTitle(string title)
	{
		UIHelper.SetLabelText(ContentsTitleLabel, title);
	}

	bool PortalContentsComponent.Listener.IsRunningEditMode()
	{
		return IsEditMode();
	}

	void PortalContentsComponent.Listener.MoveNode(int chapterIndex, int dungeonIndex)
	{
		SceneAutoBehaviour<LobbyManagement>.instance.MoveNode(chapterIndex, dungeonIndex);
		Close();
	}

	void PayShopResourceList.IListener.OnClickStamina()
	{
		if (!IsEditMode())
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Stamina.ToString());
		}
	}

	void PayShopResourceList.IListener.OnClickRuby()
	{
		if (!IsEditMode())
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
		}
	}

	void PayShopResourceList.IListener.OnClickGold()
	{
		if (!IsEditMode())
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Gold.ToString());
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
		if (changedInfo.UserStatType != UserManager.UserStatType.InventoryExtend)
		{
			PayShopResourceList.OnChanged(changedInfo);
			_contentsComponents.ForEach(delegate(PortalContentsComponent x)
			{
				x.OnChangeStaminaInfo(changedInfo);
			});
		}
	}

	void IUserObserver<NGame2.NAccount.UserPlayerCurrencyManager.ChangedCurrencyInfo>.OnChanged(NGame2.NAccount.UserPlayerCurrencyManager.ChangedCurrencyInfo changedInfo)
	{
		PayShopResourceList.OnChanged(changedInfo);
		_contentsComponents.ForEach(delegate(PortalContentsComponent x)
		{
			x.OnChangeCurrencyInfo(changedInfo);
		});
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo changedInfo)
	{
		_contentsComponents.ForEach(delegate(PortalContentsComponent x)
		{
			x.OnChangeItemCount(changedInfo);
		});
	}
}
