using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NAccount.NNotice;
using NGame2.NContents;
using NGame2.NGuild;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NWorldMap;
using NNguiExtension;
using NShared;
using NShared.NWorldBoss;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class LobbyNodeFunctionMenu : BaseWindow, IUserObserver<SoulWeaponNoticeInfo>, IUserObserver, IInspectorGuiDrawer
{
	public interface Listener
	{
		void OnClickMainButton(Action afterAction = null);

		void OnClickSubButton(NShared.NodeFunctionType function);

		void OnClickSubButton(SubButtonFunctionType buttonType, NShared.NodeFunctionType functionType, string functionValue);

		void OnClickLeaveCityButton();
	}

	[Serializable]
	public class SubButton
	{
		public SubButtonFunctionType ButtonType;

		public NShared.NodeFunctionType FunctionType;

		public string FunctionValue;

		public Transform TransformButton;

		public GameObject GoButton;

		public UISpriteSelector SpriteSelectors;
	}

	[Serializable]
	public class DungeonButton
	{
		public string ButtonName;

		public NShared.NodeFunctionType Type;

		public GameObject ButtonGo;

		public UIWidget ButtonWidget;

		public bool Hidden;
	}

	public const string PrefabPath = "Resources/Prefab2/Lobby/NodeFunctionMenu";

	public Transform MainButtonRoot;

	public Transform SubButtonRoot;

	public UIButton Button_Raid;

	public UIButton Button_WorldBoss;

	public UIWidget Widget_WorldBoss_Open;

	public UIWidget Widget_WorldBoss_Close;

	public UIButton Button_GuildArena;

	public UIButton Button_SoulWeapon;

	public GameObject SoulWeaponNotice;

	public TweenPosition Tween_ButtonMove;

	public TweenWidth Tween_BattleRewardItemMove;

	public int RewardListWidth_CountOne;

	public int RewardListWidth_CountTwoOver;

	[HideInInspector]
	public List<DungeonButton> DungeonButtons;

	public UIScrollGrid ItemGrid;

	public List<SubButton> SubButtonList;

	public int SubButtonSize;

	private List<ItemIconComponent> _battleRewardList = new List<ItemIconComponent>();

	private DungeonButton _mainButton;

	private DungeonButton _oldMainButton;

	private Listener _listener;

	private const float BUTTON_UPDATE_TIME = 1f;

	private float _lastUpdateButtonTime;

	private Dictionary<int, bool> _battleRewardRefreshedDic = new Dictionary<int, bool>();

	private GuildArenaManager _guildArenaManager;

	private GuildArenaSessionState? _prevGuildArenaSessionState = GuildArenaSessionState.None;

	[Header("[Temp Script]")]
	public ContentUISeasonFirstViewFx contentUISeasonFirstViewFx;

	public override void Awake()
	{
		base.Awake();
	}

	public void Open(Listener listener)
	{
		base.Open();
		_listener = listener;
		HideAllButton();
		ItemGrid.Grid.sorting = UIGrid.Sorting.Custom;
		ItemGrid.Grid.onCustomSort = (Transform x, Transform y) => SortReward(x, y);
		GuildArenaManager guildArenaManager = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager;
		if (guildArenaManager != null)
		{
			_guildArenaManager = guildArenaManager;
		}
		UpdateDerived();
		SetSoulWeaponNotice();
		if (AccountManager.IsLogined())
		{
			GlobalAutoBehaviour<AccountManager>.instance.AddUserObserver(this);
		}
	}

	public override bool IsOpen()
	{
		return base.IsOpen();
	}

	public override void Reset()
	{
		base.Reset();
		_mainButton = null;
		_oldMainButton = null;
		FactoryBase.ReleaseAndClearList(_battleRewardList);
		if (IsOpen())
		{
			UpdateGuildArenaButton();
		}
	}

	public override void Close()
	{
		base.Close();
		if (AccountManager.IsLogined())
		{
			GlobalAutoBehaviour<AccountManager>.instance.RemoveUserObserver(this);
		}
	}

	private void Start()
	{
	}

	public bool IsBattleButtonActivated()
	{
		if (_mainButton == null)
		{
			return false;
		}
		if (_mainButton.Type.IsBattleFunction())
		{
			return true;
		}
		return false;
	}

	private void SetupListOfRewardItemList(RewardData rewardData, BattleType battleType = BattleType.None)
	{
		FactoryBase.ReleaseAndClearList(_battleRewardList);
		int GroupCount;
		List<RewardElementData> rewardItemListWithGroupCount = rewardData.GetRewardItemListWithGroupCount(out GroupCount, ignoreRate: true);
		int num = 0;
		foreach (RewardElementData item in rewardItemListWithGroupCount)
		{
			if (item != null && GameRoot.Instance.ItemDataContainer.GetDataByCode(item.ItemCode) != null)
			{
				num++;
			}
		}
		if (Tween_BattleRewardItemMove != null)
		{
			if (num == 1)
			{
				Tween_BattleRewardItemMove.to = RewardListWidth_CountOne;
			}
			else
			{
				Tween_BattleRewardItemMove.to = RewardListWidth_CountTwoOver;
			}
			Tween_BattleRewardItemMove.PlayForward();
		}
		foreach (RewardElementData item2 in rewardItemListWithGroupCount)
		{
			if (item2 == null)
			{
				continue;
			}
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(item2.ItemCode);
			if (dataByCode != null)
			{
				int index = dataByCode.Index;
				int itemMin = item2.ItemMin;
				long minCount = itemMin;
				int itemMax = item2.ItemMax;
				long maxCount = itemMax;
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(ItemGrid, ItemIconComponent.PrefabAuto);
				if (!(itemIconComponent == null))
				{
					itemIconComponent.Open(dataByCode);
					UIHelper.ShowBonusItemEventByMinMax(itemIconComponent, index, minCount, maxCount, battleType);
					_battleRewardList.Add(itemIconComponent);
				}
			}
		}
		UIHelper.RepositionGrid(ItemGrid);
		ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, RepeatItemReposition(Tween_BattleRewardItemMove.duration));
	}

	private IEnumerator RepeatItemReposition(float time)
	{
		float t = 0f;
		while (t <= time)
		{
			t += Time.deltaTime;
			UIHelper.RepositionGrid(ItemGrid);
			yield return new WaitForFixedUpdate();
		}
	}

	private int SortReward(Transform a, Transform b)
	{
		ItemIconComponent component = a.GetComponent<ItemIconComponent>();
		ItemIconComponent component2 = b.GetComponent<ItemIconComponent>();
		ItemData itemData = ((component == null) ? null : component.ItemData);
		ItemData itemData2 = ((component2 == null) ? null : component2.ItemData);
		if (itemData == null || itemData2 == null)
		{
			if (itemData != null)
			{
				return -1;
			}
			if (itemData2 != null)
			{
				return 1;
			}
			return 0;
		}
		int num = -itemData.Grade.CompareTo(itemData2.Grade);
		if (num != 0)
		{
			return num;
		}
		return itemData.Index.CompareTo(itemData2.Index);
	}

	private void SetBattleReward(CampaignNodeData nodeData)
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(nodeData.ChapterIndex, nodeData.DungeonIndex);
		if (data == null)
		{
			return;
		}
		int difficulty = Helper.GetDifficulty(nodeData.ChapterIndex, nodeData.DungeonIndex);
		int rewardIndexOfDifficulty = data.GetRewardIndexOfDifficulty((DungeonDifficultyType)difficulty);
		RewardData data2 = GameRoot.Instance.RewardDataContainer.GetData(rewardIndexOfDifficulty);
		if (data2 == null)
		{
			UIHelper.RepositionGrid(ItemGrid);
			if (Tween_BattleRewardItemMove != null)
			{
				Tween_BattleRewardItemMove.PlayReverse();
			}
		}
		else
		{
			SetupListOfRewardItemList(data2, data.BattleType);
		}
	}

	private void RefreshGrid()
	{
		List<Tuple<ItemData, int, int>> list = new List<Tuple<ItemData, int, int>>();
		foreach (ItemIconComponent battleReward in _battleRewardList)
		{
			ItemData itemData = battleReward.ItemData;
			long itemCount = battleReward.ItemCount;
			long itemMaxCount = battleReward.ItemMaxCount;
			long num = itemCount;
			list.Add(checked(new Tuple<ItemData, int, int>(itemData, (int)num, (int)itemMaxCount)));
		}
		FactoryBase.ReleaseAndClearList(_battleRewardList);
		foreach (Tuple<ItemData, int, int> item3 in list)
		{
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(ItemGrid, ItemIconComponent.PrefabAuto);
			itemIconComponent.Open(item3.Item1);
			int item = item3.Item2;
			int item2 = item3.Item3;
			Color? color = null;
			int num2 = item2;
			int num3 = item;
			itemIconComponent.SetItemMinMax(num3, num2, color);
			_battleRewardList.Add(itemIconComponent);
		}
		UIHelper.RepositionGrid(ItemGrid);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateButton();
	}

	private void UpdateButton(bool isForceUpdate = false)
	{
		if (!AccountManager.IsLogined() || (SceneAutoBehaviour<WorldMapManagement>.instance.IsMapLoading && !isForceUpdate) || GameRoot.Instance == null || GameRoot.Instance.CampaignDungeonDataContainer == null || SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>() == null)
		{
			return;
		}
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int linkedDungeonIndex = NGame2.NAccount.UserCampaignManager.LinkedDungeonIndex;
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, linkedDungeonIndex);
		if (data == null)
		{
			return;
		}
		DungeonButton dungeonButton = SelectMainButton(data);
		if (dungeonButton != null && dungeonButton.Hidden)
		{
			dungeonButton = null;
		}
		bool flag = IsBattleButtonActivated();
		if (dungeonButton != _mainButton)
		{
			_oldMainButton.ShowButton(enabled: false);
			_oldMainButton = _mainButton;
			if (dungeonButton != null)
			{
				_mainButton.ShowButton(enabled: false);
			}
			dungeonButton.ShowButton(enabled: true);
			_mainButton = dungeonButton;
			if (IsBattleButtonActivated())
			{
				SetBattleReward(data);
			}
			if (flag && Tween_BattleRewardItemMove != null)
			{
				Tween_BattleRewardItemMove.PlayReverse();
			}
		}
		if (dungeonButton == null)
		{
			if (Tween_ButtonMove != null)
			{
				Tween_ButtonMove.PlayForward();
			}
		}
		else if (Tween_ButtonMove != null)
		{
			Tween_ButtonMove.PlayReverse();
		}
		if (Time.time - _lastUpdateButtonTime >= 1f || isForceUpdate)
		{
			_lastUpdateButtonTime = Time.time;
			UpdateSubButton();
		}
	}

	private void UpdateSubButton()
	{
		UpdateRaidButton();
		UpdateWorldBossButton();
		UpdateGuildArenaButton();
		UpdateSoulWeaponButton();
		int num = 0;
		foreach (SubButton subButton in SubButtonList)
		{
			Transform transformButton = subButton.TransformButton;
			if (!(transformButton == null) && transformButton.gameObject.activeSelf)
			{
				transformButton.localPosition = new Vector3(base.transform.localPosition.x, num * SubButtonSize, base.transform.localPosition.z);
				num++;
			}
		}
	}

	private bool IsWorldMapPlayerMoving()
	{
		if (!SceneAutoBehaviour<WorldMapManagement>.isValidInstance)
		{
			return false;
		}
		if (SceneAutoBehaviour<WorldMapManagement>.instance.IsMapLoading)
		{
			return false;
		}
		MapLayout instance = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
		if (instance == null)
		{
			return false;
		}
		return instance.IsPlayerMoving();
	}

	private void UpdateRaidButton()
	{
		if (!SingletonAuto<MenuOpenCheckManager>.instance.IsOpen(ERightMenuButton.Raid))
		{
			UIHelper.EnableGameObject(Button_Raid, enable: false);
		}
		else if (SceneAutoBehaviour<SubStoryManagement>.instance.OnSubStoryMap)
		{
			UIHelper.EnableGameObject(Button_Raid, enable: false);
		}
		else
		{
			UIHelper.EnableGameObject(Button_Raid, enable: true);
		}
	}

	private void UpdateWorldBossButton()
	{
		if (!SingletonAuto<MenuOpenCheckManager>.instance.IsOpen(ERightMenuButton.WorldBoss))
		{
			UIHelper.EnableGameObject(Button_WorldBoss, enable: false);
		}
		else if (SceneAutoBehaviour<SubStoryManagement>.instance.OnSubStoryMap)
		{
			UIHelper.EnableGameObject(Button_WorldBoss, enable: false);
		}
		else
		{
			UIHelper.EnableGameObject(Button_WorldBoss, enable: true);
		}
	}

	private void UpdateGuildArenaButton()
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager == null || GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info == null || _guildArenaManager == null)
		{
			UIHelper.EnableGameObject(Button_GuildArena, enable: false);
			return;
		}
		if (SceneAutoBehaviour<SubStoryManagement>.instance.OnSubStoryMap)
		{
			UIHelper.EnableGameObject(Button_GuildArena, enable: false);
			return;
		}
		UIHelper.EnableGameObject(Button_GuildArena, enable: true);
		GuildArenaSessionState guildArenaSessionState = GetGuildArenaState();
		if (_guildArenaManager.CheckEndTime())
		{
			guildArenaSessionState = GuildArenaSessionState.Apply;
		}
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetGuildArenaSessionState(guildArenaSessionState);
		if (_prevGuildArenaSessionState != guildArenaSessionState)
		{
			SubButton subButton = SubButtonList.Find((SubButton x) => x.ButtonType == SubButtonFunctionType.GuildArena);
			if (subButton != null)
			{
				ChangeGuildArenaIcon(subButton, guildArenaSessionState);
				_prevGuildArenaSessionState = guildArenaSessionState;
			}
		}
	}

	private GuildArenaSessionState GetGuildArenaState()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildArenaSessionInfo == null || _guildArenaManager == null)
		{
			return GuildArenaSessionState.NotHeld;
		}
		if (_guildArenaManager.IsApplyTime())
		{
			return GuildArenaSessionState.Apply;
		}
		if (_guildArenaManager.IsApplyEndedTime())
		{
			return GuildArenaSessionState.ApplyEnded;
		}
		if (_guildArenaManager.IsBattleTime())
		{
			return GuildArenaSessionState.Battle;
		}
		return GuildArenaSessionState.Ended;
	}

	private void ChangeGuildArenaIcon(SubButton subButton, GuildArenaSessionState guildArenaState)
	{
		UISpriteSelector spriteSelectors = subButton.SpriteSelectors;
		if (spriteSelectors != null)
		{
			spriteSelectors.SelectEnum(guildArenaState);
		}
		if (guildArenaState == GuildArenaSessionState.Battle)
		{
			contentUISeasonFirstViewFx.Open();
		}
		else
		{
			contentUISeasonFirstViewFx.Close();
		}
	}

	private bool ShouldEnableSoulWeaponButton()
	{
		if (!SingletonAuto<MenuOpenCheckManager>.instance.IsOpen(ERightMenuButton.SoulWeapon))
		{
			return false;
		}
		if (SceneAutoBehaviour<SubStoryManagement>.instance.OnSubStoryMap)
		{
			return false;
		}
		if (_mainButton != null && _mainButton.ButtonName == ERightMenuButton.SoulWeapon.ToString())
		{
			return false;
		}
		return true;
	}

	private void UpdateSoulWeaponButton()
	{
		bool activeInHierarchy = Button_SoulWeapon.gameObject.activeInHierarchy;
		bool flag = ShouldEnableSoulWeaponButton();
		if (activeInHierarchy != flag)
		{
			UIHelper.EnableGameObject(Button_SoulWeapon, flag);
			SetSoulWeaponNotice();
		}
	}

	private void HideAllButton()
	{
		if ((bool)MainButtonRoot)
		{
			for (int i = 0; i < MainButtonRoot.childCount; i++)
			{
				UIHelper.EnableGameObject(MainButtonRoot.GetChild(i), enable: false);
			}
		}
		if ((bool)SubButtonRoot)
		{
			UIButton[] componentsInChildren = SubButtonRoot.GetComponentsInChildren<UIButton>();
			HideAllButton(componentsInChildren);
		}
		_mainButton = null;
	}

	private void HideAllButton(UIButton[] buttons)
	{
		for (int i = 0; i < buttons.Length; i++)
		{
			UIHelper.EnableGameObject(buttons[i], enable: false);
		}
	}

	public DungeonButton SelectMainButton(CampaignNodeData nodeData)
	{
		if (nodeData == null)
		{
			return null;
		}
		if (IsWorldMapPlayerMoving())
		{
			return null;
		}
		Helper.FunctionStateData functionState = Helper.GetFunctionState(nodeData, containsDisableMsg: false);
		if (functionState.Type == NShared.NodeFunctionType.None)
		{
			return null;
		}
		if (functionState.Type == NShared.NodeFunctionType.NoEntry)
		{
			return null;
		}
		if (functionState.Type == NShared.NodeFunctionType.ShakmehDungeon)
		{
			int chapterIndex = nodeData.ChapterIndex;
			int dungeonIndex = nodeData.DungeonIndex;
			ShakmehBossData data = GameRoot.Instance.ShakmehBossDataContainer.GetData(chapterIndex, dungeonIndex);
			NGame2.NUI.NManager.ShakmehDungeonManagement instance = SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.instance;
			if (data != null && (bool)instance)
			{
				switch (data.BossType)
				{
				case ShakmehBossType.Middle:
					if (instance.IsEnoughShakmehBossGauge)
					{
						return null;
					}
					break;
				case ShakmehBossType.Final:
					if (!instance.IsEnoughShakmehBossGauge)
					{
						return null;
					}
					break;
				}
			}
		}
		if (functionState.State == Helper.FunctionStateData.EFunctionState.Disabled)
		{
			return null;
		}
		DungeonButton dungeonButton = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(DungeonButtons); i++)
		{
			DungeonButton dungeonButton2 = DungeonButtons[i];
			if (string.Equals(dungeonButton2.ButtonName, functionState.Button))
			{
				dungeonButton = dungeonButton2;
				break;
			}
			if (dungeonButton == null && dungeonButton2.Type == functionState.Type)
			{
				dungeonButton = dungeonButton2;
			}
		}
		if (dungeonButton == null)
		{
			return null;
		}
		dungeonButton.EnableButton(functionState.State == Helper.FunctionStateData.EFunctionState.Enabled);
		return dungeonButton;
	}

	private DungeonButton SelectSubButton(CampaignNodeData nodeData)
	{
		return null;
	}

	public void OnClickButton()
	{
		if (_listener != null && UICamera.currentTouch != null)
		{
			GameObject current = UICamera.currentTouch.current;
			if (_mainButton != null && _mainButton.ButtonGo == current)
			{
				_listener.OnClickMainButton();
			}
		}
	}

	public void OnClickSubButton(GameObject buttonGo)
	{
		if (_listener != null && !(buttonGo == null))
		{
			SubButton subButton = SubButtonList.Find((SubButton button) => button.GoButton == buttonGo);
			if (subButton != null)
			{
				_listener.OnClickSubButton(subButton.ButtonType, subButton.FunctionType, subButton.FunctionValue);
			}
		}
	}

	public void OnClickLeaveCityButton()
	{
		if (_listener != null)
		{
			_listener.OnClickLeaveCityButton();
		}
	}

	public void OnClickNode()
	{
		if (_listener != null && _mainButton != null)
		{
			_listener.OnClickMainButton();
		}
	}

	public void OnChangeWorldBossStatus(WorldBossStatus status)
	{
		if (status == WorldBossStatus.ActiveDeal || status == WorldBossStatus.ActiveNormal)
		{
			UIHelper.EnableGameObject(Widget_WorldBoss_Open, enable: true);
			UIHelper.EnableGameObject(Widget_WorldBoss_Close, enable: false);
		}
		else
		{
			UIHelper.EnableGameObject(Widget_WorldBoss_Open, enable: false);
			UIHelper.EnableGameObject(Widget_WorldBoss_Close, enable: true);
		}
	}

	private void SetSoulWeaponNotice()
	{
		if (UIHelper.IsEnabledSelfGameObjet(Button_SoulWeapon))
		{
			bool enable = false;
			if (AccountManager.IsLogined())
			{
				enable = GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponNoticeManager.AnyNoticeStateOn();
			}
			UIHelper.EnableGameObject(SoulWeaponNotice, enable);
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<SoulWeaponNoticeInfo>.OnChanged(SoulWeaponNoticeInfo changedInfo)
	{
		SetSoulWeaponNotice();
	}
}
