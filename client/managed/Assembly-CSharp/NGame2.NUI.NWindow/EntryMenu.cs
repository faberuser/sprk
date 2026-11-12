using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NEventTrigger;
using NGame2.NFunctionality;
using NGame2.NScene;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NEntryMenu;
using NGameAsset;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class EntryMenu : BaseWindow, LobbyTopmostMenu.ILobbyTopmostObserver<GuildResourceObserverValue>, LobbyTopmostMenu.IPublicObserver, LobbyTopmostMenu.ICustomStaminaObserver, LobbyTopmostMenu.IUserStatObserver, LobbyTopmostMenu.IMiniGuideButtonObserver
{
	public interface Listener
	{
		bool OnCloseRequest();

		void SetHideEntryMenuTitle(bool Hide);

		void ExecNPCAction(string[] actionValues, bool playsound);
	}

	[Serializable]
	public class ReturnBgmInfo
	{
		public string EntryMenu;

		public string BgmCode;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/EntryMenu/EntryMenu";

	public WindowAnimation ExitButtonAnimation;

	public UITable TableGrid;

	public UISimpleGrid BottomButtonGrid;

	public UISimpleGrid BuffIconGrid;

	public UIWidget WidgetSpeakBubble;

	public UILabel Label_Speak;

	public TweenAlpha BubbleTweenAlpha;

	public UIWidget WidgetMenuButtons;

	public UIWidget WidgetBottomMenuButtons;

	public UIWidget WidgetBuffIcons;

	public List<EntryMenuButtonComponent> _BottomButtons = new List<EntryMenuButtonComponent>();

	public List<ReturnBgmInfo> ReturnBgmEntryMenuList;

	protected CampaignNodeData _nodeData;

	protected NPCData _NPCData;

	protected List<EntryMenuButtonComponent> _ListButtons = new List<EntryMenuButtonComponent>();

	protected string _EntryNPCCode;

	private List<GeneralIconComponent> _buffIcons = new List<GeneralIconComponent>();

	private List<GeneralIconComponent> _removebuffIcons = new List<GeneralIconComponent>();

	protected Listener _listener;

	private DateTime _NPCTouchedTime = DateTime.MinValue;

	private float _NPCActionDelaySecond = 3f;

	private GuildResourceObserverValue _tempGuildResourceObserverValue = new GuildResourceObserverValue();

	GuildResourceObserverValue LobbyTopmostMenu.ILobbyTopmostObserver<GuildResourceObserverValue>.BaseValue
	{
		get
		{
			if (_nodeData == null)
			{
				_tempGuildResourceObserverValue.Value = GuildConstructionResourceTopmostMenuType.None;
			}
			else
			{
				_tempGuildResourceObserverValue.Value = _nodeData.GuildResourceMenuType;
			}
			return _tempGuildResourceObserverValue;
		}
	}

	LobbyTopmostMenu.PublicObserverType LobbyTopmostMenu.ILobbyTopmostObserver<GuildResourceObserverValue>.Type => LobbyTopmostMenu.PublicObserverType.ShowGUildResource;

	public StaminaType StaminaType
	{
		get
		{
			if (_nodeData == null)
			{
				return StaminaType.Chicken;
			}
			StaminaType staminaType = _nodeData.EntryMenuStamina;
			if (staminaType == StaminaType.None)
			{
				staminaType = StaminaType.Chicken;
			}
			return staminaType;
		}
	}

	public UserManager.UserStatType StatType
	{
		get
		{
			if (_nodeData == null)
			{
				return UserManager.UserStatType.Gold;
			}
			if (_nodeData.EntryMenuAsset == ShopCostType.FriendshipPoint)
			{
				return UserManager.UserStatType.FriendshipPoint;
			}
			return UserManager.UserStatType.Gold;
		}
	}

	public new int WindowGuideIndex
	{
		get
		{
			if (_NPCData == null)
			{
				return 0;
			}
			return _NPCData.MiniGuideIndex;
		}
	}

	public void Init(Listener listener, object nodeData, bool playNPCSound = false, bool isIgnoreTrigger = false, Action complete = null)
	{
		bool flag = nodeData.GetType() == typeof(CampaignNodeData);
		if (flag)
		{
			_nodeData = nodeData as CampaignNodeData;
			_EntryNPCCode = _nodeData.EntryNPCCode;
			if (!InspectNodeData(_nodeData))
			{
				Vespa_System_Action_Extension_Method.fire(complete);
				return;
			}
		}
		else
		{
			_nodeData = null;
			_EntryNPCCode = nodeData.ToString();
		}
		_NPCData = GameRoot.Instance.NPCDataContainer.GetData(_EntryNPCCode);
		if (_NPCData == null)
		{
			Vespa_System_Action_Extension_Method.fire(complete);
			return;
		}
		_listener = listener;
		if (ExitButtonAnimation != null)
		{
			float duration = ExitButtonAnimation.Duration;
			ExitButtonAnimation.AdjustAll(duration);
		}
		SetupMenuButtonGroup();
		SetupBottomMenuButtonGroup();
		SetupBuffIconGroup();
		SetNPCWelcomeAction(playNPCSound);
		RaiseOrdealTowerEvent(isIgnoreTrigger);
		if (flag)
		{
			int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
			int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
			SceneAutoBehaviour<ContentsNoticePopupManagement>.instance.OpenContentsNodeStartPopup(chapterIndex, dungeonIndex);
		}
		Vespa_System_Action_Extension_Method.fire(complete);
	}

	private bool InspectNodeData(CampaignNodeData nodeData)
	{
		if (nodeData == null || String_Extension_Method.IsNullOrEmpty(nodeData.EntryNPCCode))
		{
			return false;
		}
		if (NGame2.NAccount.UserCampaignManager.ChapterIndex != nodeData.ChapterIndex || NGame2.NAccount.UserCampaignManager.DungeonIndex != nodeData.DungeonIndex)
		{
			GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Lobby);
			return false;
		}
		return true;
	}

	public override void Reset()
	{
		base.Reset();
		SetHideButtons(Hide: false);
		ClearMenuButtons();
		ClearBuffIcon();
		SetSpeech(string.Empty);
		if (BubbleTweenAlpha != null)
		{
			BubbleTweenAlpha.ResetToBeginning();
		}
		_listener = null;
	}

	private void ClearMenuButtons()
	{
		for (int i = 0; i < _ListButtons.Count; i++)
		{
			_ListButtons[i].Close();
		}
		FactoryBase.ReleaseAndClearList(_ListButtons);
	}

	private void ClearBuffIcon()
	{
		FactoryBase.ReleaseAndClearList(_buffIcons);
	}

	private void ReturnBgm()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(ReturnBgmEntryMenuList) || _NPCData == null)
		{
			return;
		}
		ReturnBgmInfo returnBgmInfo = null;
		for (int i = 0; i < ReturnBgmEntryMenuList.Count; i++)
		{
			returnBgmInfo = ReturnBgmEntryMenuList[i];
			if (returnBgmInfo != null && _NPCData.Code == returnBgmInfo.EntryMenu)
			{
				SingletonAuto<BgmManager2>.instance.StopEvent(SingletonAuto<BgmManager2>.instance.GetCurrentEventAssetBgmCode());
				SingletonAuto<BgmManager2>.instance.Request(returnBgmInfo.BgmCode, isEvent: true);
				break;
			}
		}
	}

	public override void ReOpen()
	{
		base.ReOpen();
		if ((bool)ExitButtonAnimation)
		{
			ExitButtonAnimation.PlayAnimation(reverse: false);
		}
		if (_listener != null)
		{
			_listener.SetHideEntryMenuTitle(Hide: false);
		}
	}

	public void UpdateEntryMenuButton()
	{
		ClearMenuButtons();
		SetupMenuButtonGroup();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateBuffIcon();
	}

	public static bool CheckEntryMenuConditionAndNodeData(CampaignNodeData nodeData, string conditionData)
	{
		if (nodeData == null)
		{
			return true;
		}
		return CheckEntryMenuCondition(nodeData, conditionData);
	}

	public void OpenBottomMenu(int buttonIndex)
	{
		if (buttonIndex < _BottomButtons.Count)
		{
			_BottomButtons[buttonIndex].OnClickButton();
		}
	}

	public void OpenEntryMenu(int buttonIndex)
	{
		if (buttonIndex < _ListButtons.Count)
		{
			_ListButtons[buttonIndex].OnClickButton();
		}
	}

	public void OpenEntryMenuByShopIndex(string compareString, int shopIndex, int? forceGroupIndex = null)
	{
		ShopData data = GameRoot.Instance.ShopDataContainer.GetData(shopIndex);
		if (data == null)
		{
			return;
		}
		string code = data.Code;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_ListButtons); i++)
		{
			string[] entryMenuButton = _ListButtons[i].GetEntryMenuData.EntryMenuButton;
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(entryMenuButton) - 1; j++)
			{
				if (entryMenuButton[j] == compareString && entryMenuButton[j + 1] == code)
				{
					if (forceGroupIndex.HasValue)
					{
						SceneAutoBehaviour<EntryMenuManagement>.instance.ForceShopGroupIndex = forceGroupIndex;
					}
					else
					{
						SceneAutoBehaviour<EntryMenuManagement>.instance.ForceShopGroupIndex = null;
					}
					_ListButtons[i].OnClickButton();
				}
			}
		}
	}

	public static bool CheckEntryMenuCondition(CampaignNodeData nodeData, string conditionData)
	{
		if (conditionData == null || conditionData.Length == 0)
		{
			return true;
		}
		string[] array = conditionData.Split(new char[1] { ';' });
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(array); i++)
		{
			string[] array2 = array[i].Split(new char[1] { ',' });
			if (__Extension_Method_ICollection__.COUNT(array2) != 0)
			{
				string text = array2[0].Trim();
				ICondition condition = SingletonAuto<ConditionContainer>.instance.GetCondition(text);
				if (condition != null && !condition.Check(nodeData, array2))
				{
					return false;
				}
			}
		}
		return true;
	}

	protected virtual bool CheckCondition(string conditionData)
	{
		return CheckEntryMenuConditionAndNodeData(_nodeData, conditionData);
	}

	private void UpdateBuffIcon()
	{
		FindInvalidBuffIcon();
		RemoveInvalidBuffIcon();
	}

	private void FindInvalidBuffIcon()
	{
		if (__Extension_Method_ICollection__.COUNT(_buffIcons) == 0)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_buffIcons); i++)
		{
			GeneralIconComponent generalIconComponent = _buffIcons[i];
			if (!generalIconComponent.IsEffective())
			{
				_removebuffIcons.Add(generalIconComponent);
			}
		}
	}

	private void RemoveInvalidBuffIcon()
	{
		if (__Extension_Method_ICollection__.COUNT(_removebuffIcons) != 0)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_removebuffIcons); i++)
			{
				GeneralIconComponent comp = _removebuffIcons[i];
				_buffIcons.Remove(comp);
				FactoryBase.SafeReleaseComponent(ref comp);
			}
			_removebuffIcons.Clear();
		}
	}

	protected virtual void SetupMenuButtonGroup()
	{
		if (_NPCData != null)
		{
			int num = 1;
			while (true)
			{
				NShared.EntryMenu entryMenuButton = _NPCData.GetEntryMenuButton(num);
				if (entryMenuButton == null || !entryMenuButton.IsValid)
				{
					break;
				}
				if (!entryMenuButton.IsChild && CheckCondition(entryMenuButton.EntryMenuCondition))
				{
					SetupMenuButton(entryMenuButton);
				}
				num++;
			}
		}
		RepositionTableGrid();
	}

	protected void SetupBottomMenuButtonGroup()
	{
		_BottomButtons.ForEach(delegate(EntryMenuButtonComponent bottom)
		{
			UIHelper.EnableGameObject(bottom, enable: false);
		});
		if (_NPCData != null)
		{
			int num = 1;
			while (true)
			{
				NShared.EntryMenu bottomEntryMenu = _NPCData.GetBottomEntryMenu(num);
				if (bottomEntryMenu == null || !bottomEntryMenu.IsValid)
				{
					break;
				}
				if (CheckCondition(bottomEntryMenu.EntryMenuCondition))
				{
					SetupBottomMenuButton(bottomEntryMenu);
				}
				num++;
			}
		}
		UIHelper.RepositionGrid(BottomButtonGrid);
	}

	protected virtual void SetupMenuButton(NShared.EntryMenu menu)
	{
		string valueString = ArgParser.GetValueString(menu.EntryMenuButton, 1);
		if (String_Extension_Method.IsNullOrEmpty(valueString))
		{
			return;
		}
		NGame2.NUI.NWindow.NEntryMenu.IAction action = SingletonAuto<ActionContainer>.instance.GetAction(valueString);
		if (action == null)
		{
			return;
		}
		EntryMenuButtonComponent entryMenuButtonComponent = EntryMenuButtonComponent.PrefabAuto.Factory.Create(TableGrid.transform);
		if (_nodeData != null)
		{
			entryMenuButtonComponent.Open(_NPCData, menu, action.GetType(), delegate
			{
				action.Run(_nodeData, menu);
			});
		}
		else
		{
			entryMenuButtonComponent.Open(_NPCData, menu, action.GetType(), delegate
			{
				action.Run(menu, new List<object> { _EntryNPCCode });
			});
		}
		_ListButtons.Add(entryMenuButtonComponent);
	}

	protected virtual void SetupBottomMenuButton(NShared.EntryMenu menu)
	{
		string valueString = ArgParser.GetValueString(menu.EntryMenuButton, 1);
		if (String_Extension_Method.IsNullOrEmpty(valueString))
		{
			return;
		}
		NGame2.NUI.NWindow.NEntryMenu.IAction action = SingletonAuto<ActionContainer>.instance.GetAction(valueString);
		if (action == null)
		{
			return;
		}
		string buttonName = ArgParser.GetValueString(menu.EntryMenuButton, 2);
		EntryMenuButtonComponent entryMenuButtonComponent = _BottomButtons.Find((EntryMenuButtonComponent b) => b.name == buttonName);
		if (entryMenuButtonComponent == null)
		{
			return;
		}
		entryMenuButtonComponent.gameObject.SetActive(value: true);
		if (_nodeData != null)
		{
			entryMenuButtonComponent.Open(_NPCData, menu, action.GetType(), delegate
			{
				action.Run(_nodeData, menu);
			});
			return;
		}
		entryMenuButtonComponent.Open(_NPCData, menu, action.GetType(), delegate
		{
			action.Run(menu, new List<object> { _EntryNPCCode });
		});
	}

	public void ExecButtonAction(Type actionType)
	{
		EntryMenuButtonComponent entryMenuButtonComponent = _ListButtons.Find((EntryMenuButtonComponent comp) => comp.ActionType == actionType);
		if (!(entryMenuButtonComponent == null))
		{
			entryMenuButtonComponent.OnClickButton();
		}
	}

	protected void SetupBuffIconGroup()
	{
		ClearBuffIcon();
		if (_NPCData == null)
		{
			return;
		}
		if (_NPCData.ShowHeroBuffIcon && _NPCData.HeroIndex > 0)
		{
			CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(_NPCData.HeroIndex);
			if (cachedHeroInfo != null)
			{
				HeroBonusData data = GameRoot.Instance.HeroBonusDataContainer.GetData(cachedHeroInfo.HeroIndex, cachedHeroInfo.Star, cachedHeroInfo.Transcended);
				if (data != null)
				{
					GeneralIconComponent generalIconComponent = BuffIconGrid.AddGridItem(GeneralIconComponent.PrefabAuto);
					generalIconComponent.Open(UIHelper.GetAttrTypeIconPath(data.Icon), data, GeneralIconComponent.DecoType.EntryMenuBuff);
					_buffIcons.Add(generalIconComponent);
				}
			}
		}
		UIHelper.CreatreNpcBuffIconComponent(_NPCData, _buffIcons, BuffIconGrid);
		UIHelper.RepositionGrid(BuffIconGrid);
	}

	protected void SetNPCWelcomeAction(bool playSound)
	{
		if (_NPCData != null && GlobalAutoBehaviour<FunctionalityManager>.instance.Check(FunctionalityType.EntryMenuNpcWelcomAction, value: true) && _listener != null)
		{
			_listener.ExecNPCAction(_NPCData.OnOpenEntryMenu, playSound);
			_NPCTouchedTime = DateTime.UtcNow;
		}
	}

	public virtual void OnClickCloseButton()
	{
		if (_listener != null)
		{
			_listener.OnCloseRequest();
			Close();
			ReturnBgm();
		}
	}

	public override void OnSystemBackButton()
	{
		if (_nodeData != null)
		{
			LeaveEntryMenu leaveEntryMenu = new LeaveEntryMenu();
			leaveEntryMenu.ChapterIndex = _nodeData.ChapterIndex;
			leaveEntryMenu.DungeonIndex = _nodeData.DungeonIndex;
			SingletonAuto<EventTriggerManager>.instance.RaiseEvent(leaveEntryMenu);
		}
		OnClickCloseButton();
	}

	private void HideAll()
	{
		SetHideButtons(Hide: true);
	}

	public override void OnOtherWindowOpened()
	{
		HideAll();
		if (_listener != null)
		{
			_listener.SetHideEntryMenuTitle(Hide: true);
		}
	}

	public void SetSpeech(string speech)
	{
		bool flag = !String_Extension_Method.IsNullOrEmpty(speech);
		UIHelper.EnableGameObject(WidgetSpeakBubble, flag);
		if (!flag)
		{
			return;
		}
		UIHelper.SetLabelText(Label_Speak, speech);
		if (BubbleTweenAlpha != null)
		{
			BubbleTweenAlpha.ResetToBeginning();
			BubbleTweenAlpha.PlayForward();
			BubbleTweenAlpha.SetOnFinished(delegate
			{
				UIHelper.EnableGameObject(WidgetSpeakBubble, enable: false);
			});
		}
	}

	public void OnTouchNPC()
	{
		if (_listener != null)
		{
			DateTime utcNow = DateTime.UtcNow;
			if (!((utcNow - _NPCTouchedTime).TotalSeconds < (double)_NPCActionDelaySecond))
			{
				_NPCTouchedTime = utcNow;
				_listener.ExecNPCAction(_NPCData.OnTouch, playsound: true);
			}
		}
	}

	public void SetHideButtons(bool Hide)
	{
		UIHelper.EnableGameObject(WidgetMenuButtons, !Hide);
		UIHelper.EnableGameObject(WidgetBottomMenuButtons, !Hide);
		UIHelper.EnableGameObject(WidgetBuffIcons, !Hide);
	}

	public void RepositionTableGrid()
	{
		if (!(TableGrid == null))
		{
			TableGrid.Reposition();
		}
	}

	public NPCData GetNpcData()
	{
		return _NPCData;
	}

	public void RaiseOrdealTowerEvent(bool isIgnoreTrigger)
	{
		if (_nodeData == null || isIgnoreTrigger)
		{
			return;
		}
		bool maxStarHeroExist = false;
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxStar");
		foreach (CachedHeroInfo item in GlobalAutoBehaviour<AccountManager>.instance.HeroManager)
		{
			if (item.Star >= valueInt)
			{
				maxStarHeroExist = true;
				break;
			}
		}
		OpenTowerOfOrdeal openTowerOfOrdeal = new OpenTowerOfOrdeal();
		openTowerOfOrdeal.ChapterIndex = _nodeData.ChapterIndex;
		openTowerOfOrdeal.DungeonIndex = _nodeData.DungeonIndex;
		openTowerOfOrdeal.SetMaxStarHeroExist(maxStarHeroExist);
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(openTowerOfOrdeal);
	}
}
