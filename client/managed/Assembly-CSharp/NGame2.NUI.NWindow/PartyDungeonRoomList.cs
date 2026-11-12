using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager.NLobby;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class PartyDungeonRoomList : BaseWindow, PartyDungeonRoomComponent.Listener, LobbyTopmostMenu.IUserStatObserver
{
	public interface Listener
	{
		void OnClickEnterDungeon(RaidRoomInfo roomInfo);

		void RefreshList(PartyDungeonType dungeonType, int chapterIndex, int dungeonIndex, PartyDungeonDifficultyType diff, Action onResponse = null);

		void OnClickPartyCreate();

		void OnClickAutoParty(PartyDungeonType dungeonType, int chapterIndex, int dungeonIndex, PartyDungeonDifficultyType diff);
	}

	private enum SelectedItemType
	{
		None,
		Type,
		Chapter,
		Stage
	}

	public const string PrefabPath = "Resources/Prefab2/Window/PartyDungeon/PartyDungeonRoomList";

	private int RefreshTime = 5;

	public UILabel TitleLabel;

	public UILabel RefreshTimeLabel;

	public UIButton RefreshButton;

	public UIScrollGrid RoomGrid;

	private List<PartyDungeonData> _selectedDataList = new List<PartyDungeonData>();

	private List<PartyDungeonRoomComponent> _partyDungeonRoomList = new List<PartyDungeonRoomComponent>();

	private RaidRoomInfo[] RoomInfos;

	private Listener _Listener;

	private int RaidIndex;

	public UIComboBox ComboBox_Type;

	public UIComboBox ComboBox_Chapter;

	public UIComboBox ComboBox_Stage;

	private SortedDictionary<PartyDungeonType, bool> Dics_DungeonType = new SortedDictionary<PartyDungeonType, bool>();

	private SortedDictionary<PartyDungeonChapter, bool> Dics_Chapter = new SortedDictionary<PartyDungeonChapter, bool>();

	private SortedDictionary<int, bool> Dics_Stage = new SortedDictionary<int, bool>();

	private Dictionary<int, string> Stage_Locale = new Dictionary<int, string>();

	private Queue<RaidRoomInfo> _queue_RoomInfo = new Queue<RaidRoomInfo>();

	private float _reqSearchListCoolTime = 30f;

	private bool _ableReqSearchListBeforeAutoParty = true;

	private bool _onUpdateComboBox;

	public UserManager.UserStatType StatType => UserManager.UserStatType.Gold;

	private void SetRoomList(RaidRoomInfo[] roomInfos)
	{
		_queue_RoomInfo.Clear();
		RoomInfos = roomInfos;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(RoomInfos); i++)
		{
			if (IsEnterAbleRoom(RoomInfos[i]))
			{
				_queue_RoomInfo.Enqueue(RoomInfos[i]);
			}
		}
	}

	public override void Awake()
	{
		base.Awake();
		RefreshTime = GameRoot.Instance.ConstantDataContainer.GetValueInt("RaidRoomListRefreshDelayTime");
	}

	public void Open(RaidRoomInfo[] roomInfos, Listener listener)
	{
		if (listener == null)
		{
			VespaDebug.LogError("Listener is null");
			return;
		}
		_Listener = listener;
		SetRoomList(roomInfos);
		base.Open();
		Init();
	}

	public void Init()
	{
		CreateList(RoomInfos);
		ResetRefreshButton();
		SetComboBoxCallback();
		UpdateComboBox(SelectedItemType.None);
		ComboBox_Type.SelectItem(1);
	}

	private void ClearComboBoxItem()
	{
		ComboBox_Type.Clear();
		ComboBox_Chapter.Clear();
		ComboBox_Stage.Clear();
	}

	public override void Reset()
	{
		base.Reset();
		_selectedDataList.Clear();
		ClearList();
		ClearItemDics(SelectedItemType.None);
		ClearComboBoxItem();
	}

	private void CreateList(RaidRoomInfo[] roomInfos)
	{
		if (roomInfos != null)
		{
			foreach (RaidRoomInfo roomInfo in roomInfos)
			{
				PartyDungeonRoomComponent partyDungeonRoomComponent = UIHelper.AddGridItem(RoomGrid, PartyDungeonRoomComponent.PrefabAuto);
				partyDungeonRoomComponent.Open(roomInfo, this, IsEnterAbleRoom(roomInfo));
				_partyDungeonRoomList.Add(partyDungeonRoomComponent);
			}
			UIHelper.RepositionGrid(RoomGrid);
		}
	}

	private void ClearList()
	{
		FactoryBase.ReleaseAndClearList(_partyDungeonRoomList);
	}

	public void RefreshList(RaidRoomInfo[] roomInfos)
	{
		SetRoomList(roomInfos);
		CreateList(roomInfos);
	}

	private IEnumerator CheckRefresh()
	{
		UIHelper.EnableButton(RefreshButton, enable: false, grayscaleIfDisable: true);
		float time = Time.time;
		while (true)
		{
			float num = Time.time - time;
			int num2 = (int)Mathf.Ceil((float)RefreshTime - num);
			string stringDirect = LocalizationManager.GetStringDirect("RAID_ROOM_LIST_RESET_TIMER", num2);
			UIHelper.SetLabelText(RefreshTimeLabel, stringDirect);
			if (num >= (float)RefreshTime)
			{
				break;
			}
			yield return null;
		}
		ResetRefreshButton();
	}

	private void ResetRefreshButton()
	{
		string stringDirect = LocalizationManager.GetStringDirect("RAID_ROOM_LIST_RESET");
		UIHelper.SetLabelText(RefreshTimeLabel, string.Format(stringDirect));
		UIHelper.EnableButton(RefreshButton, enable: true, grayscaleIfDisable: true);
	}

	private void GetRequestParam(out PartyDungeonType dungeonType, out int chapterIndex, out int dungeonIndex, out PartyDungeonDifficultyType diff)
	{
		PartyDungeonType currentValue = GetCurrentValue(ComboBox_Type, PartyDungeonType.None);
		PartyDungeonChapter currentValue2 = GetCurrentValue(ComboBox_Chapter, PartyDungeonChapter.None);
		diff = PartyDungeonDifficultyType.None;
		dungeonIndex = GetCurrentValue(ComboBox_Stage, 0);
		dungeonType = ((currentValue == PartyDungeonType.None) ? PartyDungeonType.Campaign : currentValue);
		chapterIndex = ((currentValue2 != PartyDungeonChapter.None) ? _selectedDataList[0].ChapterIndex : 0);
	}

	void PartyDungeonRoomComponent.Listener.OnClickEnterDungeon(RaidRoomInfo roomInfo)
	{
		if (_Listener != null)
		{
			_Listener.OnClickEnterDungeon(roomInfo);
		}
	}

	public void OnClickRefresh(Action onResponse = null)
	{
		ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, CheckRefresh());
		RequestRefreshList(onResponse);
	}

	private void RequestRefreshList(Action onResponse)
	{
		ClearList();
		if (_Listener != null && __Extension_Method_ICollection__.COUNT(_selectedDataList) > 0)
		{
			GetRequestParam(out var dungeonType, out var chapterIndex, out var dungeonIndex, out var diff);
			_Listener.RefreshList(dungeonType, chapterIndex, dungeonIndex, diff, onResponse);
		}
	}

	public void OnClickPartyCreate()
	{
		if (SceneAutoBehaviour<MultiPartyManagement>.instance.CheckEnterBattle() && _Listener != null)
		{
			_Listener.OnClickPartyCreate();
		}
	}

	private IEnumerator CoCoolTimeAutoPartySearchList()
	{
		_onUpdateComboBox = false;
		_ableReqSearchListBeforeAutoParty = false;
		float time = Time.time;
		while (!(Time.time - time >= _reqSearchListCoolTime) && !_onUpdateComboBox)
		{
			yield return null;
		}
		_onUpdateComboBox = false;
		_ableReqSearchListBeforeAutoParty = true;
	}

	private bool NeedAutoPartySearchRefresh()
	{
		_reqSearchListCoolTime = GameRoot.Instance.ConstantDataContainer.GetValueInt("AutoSearchRefreshCooltime");
		if (_reqSearchListCoolTime == 0f)
		{
			_reqSearchListCoolTime = 30f;
		}
		if (GameRoot.Instance.ConstantDataContainer.GetValueBool("SkipSeachListAutoParty"))
		{
			return false;
		}
		return _ableReqSearchListBeforeAutoParty;
	}

	public void OnClickAutoParty()
	{
		if (_Listener != null)
		{
			if (NeedAutoPartySearchRefresh())
			{
				ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, CoCoolTimeAutoPartySearchList());
				RequestRefreshList(CheckQAndStartAutoPartySearch);
			}
			else
			{
				CheckQAndStartAutoPartySearch();
			}
		}
	}

	private void CheckQAndStartAutoPartySearch()
	{
		if (_queue_RoomInfo.Count > 0)
		{
			if (SceneAutoBehaviour<MultiPartyManagement>.instance.CheckEnterBattle())
			{
				RaidRoomInfo roomInfo = _queue_RoomInfo.Dequeue();
				_Listener.OnClickEnterDungeon(roomInfo);
			}
		}
		else if (__Extension_Method_ICollection__.COUNT(_selectedDataList) > 0)
		{
			AutoPartyDungeonPopup();
		}
	}

	private void AutoPartyDungeonPopup()
	{
		string stringDirect = LocalizationManager.GetStringDirect("PARTY_SEARCH");
		string stringDirect2 = LocalizationManager.GetStringDirect("PARTY_SEARCH_DES");
		MessagePopup3Button.Open(stringDirect, stringDirect2, delegate
		{
			GetRequestParam(out var dungeonType, out var chapterIndex, out var dungeonIndex, out var diff);
			_Listener.OnClickAutoParty(dungeonType, chapterIndex, dungeonIndex, diff);
		}, null, LocalizationManager.GetStringDirect("COMMON_CONFIRM"), LocalizationManager.GetStringDirect("COMMON_CANCEL"));
	}

	private void SetComboBoxCallback()
	{
		ComboBox_Type.OnChanged = OnDungeonSelect;
		ComboBox_Chapter.OnChanged = OnChapterSelect;
		ComboBox_Stage.OnChanged = OnStageSelect;
	}

	private void SetComboBoxItem<T>(UIComboBox comboBox, IDictionary<T, bool> list, Func<T, string> localeFunc)
	{
		comboBox.Clear();
		foreach (KeyValuePair<T, bool> item in list)
		{
			if (item.Value)
			{
				string text = ((localeFunc != null) ? localeFunc(item.Key) : string.Empty);
				comboBox.AddItem((int)(object)item.Key, text);
			}
		}
		comboBox.Reposition();
	}

	private void ClearIf(IDictionary dics, bool condition)
	{
		if (condition)
		{
			dics.Clear();
		}
	}

	private T GetCurrentValue<T>(UIComboBox comboBox, T defaultValue = default(T))
	{
		if (comboBox.CurrentKey != -1)
		{
			return (T)(object)comboBox.CurrentKey;
		}
		return defaultValue;
	}

	private void SelectDefaultItemIf(UIComboBox comboBox, int value, bool condition)
	{
		if (condition)
		{
			comboBox.SelectItem(value, isNotify: false);
		}
	}

	private void ClearItemDics(SelectedItemType selectedType)
	{
		ClearIf(Dics_DungeonType, selectedType < SelectedItemType.Type);
		ClearIf(Dics_Chapter, selectedType < SelectedItemType.Chapter);
		ClearIf(Dics_Stage, selectedType < SelectedItemType.Stage);
	}

	private void UpdateItemDics(SelectedItemType selectedType)
	{
		ClearItemDics(selectedType);
		SelectDefaultItemIf(ComboBox_Type, 0, selectedType < SelectedItemType.Type);
		SelectDefaultItemIf(ComboBox_Chapter, 0, selectedType < SelectedItemType.Chapter);
		SelectDefaultItemIf(ComboBox_Stage, 0, selectedType < SelectedItemType.Stage);
		PartyDungeonType currentValue = GetCurrentValue(ComboBox_Type, PartyDungeonType.None);
		PartyDungeonChapter currentValue2 = GetCurrentValue(ComboBox_Chapter, PartyDungeonChapter.None);
		int currentValue3 = GetCurrentValue(ComboBox_Stage, 0);
		Dics_Chapter[currentValue2] = true;
		Dics_Stage[currentValue3] = true;
		if (selectedType < SelectedItemType.Stage)
		{
			Stage_Locale.Clear();
		}
		_selectedDataList.Clear();
		PartyDungeonDifficultyType reqDiff = PartyDungeonDifficultyType.None;
		List<PartyDungeonData> filteredData = GameRoot.Instance.PartyDungeonDataContainer.GetFilteredData(currentValue, currentValue2, reqDiff, currentValue3, delegate(PartyDungeonData data)
		{
			if (data == null)
			{
				return false;
			}
			return GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanGoToChapter(data.ChapterIndex) && GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(data.ChapterIndex, data.DungeonIndex);
		});
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(filteredData); num++)
		{
			Dics_DungeonType[filteredData[num].DungeonType] = true;
			Dics_Chapter[filteredData[num].Chapter] = true;
			if (currentValue2 != PartyDungeonChapter.None)
			{
				Dics_Stage[filteredData[num].DungeonIndex] = true;
				Stage_Locale[filteredData[num].DungeonIndex] = UIHelper.GetNodeName(filteredData[num].ChapterIndex, filteredData[num].DungeonIndex);
			}
			Helper.GetMaxAvailableDifficulty(filteredData[num].ChapterIndex, filteredData[num].DungeonIndex);
			_selectedDataList.Add(filteredData[num]);
		}
	}

	private void SelectComboBoxItem(UIComboBox combo)
	{
		if (combo.CurrentKey == -1)
		{
			combo.SelectFirstItem(isNotify: false);
		}
		else
		{
			combo.SelectItem(combo.CurrentKey, isNotify: false);
		}
	}

	private string GetStageLocale(int stage)
	{
		if (stage == 0)
		{
			return UIHelper.PartyStageLocale(stage);
		}
		if (Stage_Locale.ContainsKey(stage))
		{
			return Stage_Locale[stage];
		}
		return stage.ToString();
	}

	private void UpdateComboBox(SelectedItemType selectedItemType)
	{
		_onUpdateComboBox = true;
		UpdateItemDics(selectedItemType);
		SetComboBoxItem(ComboBox_Type, Dics_DungeonType, UIHelper.PartyDungeonTypeLocale);
		SetComboBoxItem(ComboBox_Chapter, Dics_Chapter, UIHelper.PartyChapterLocale);
		SetComboBoxItem(ComboBox_Stage, Dics_Stage, GetStageLocale);
		SelectComboBoxItem(ComboBox_Type);
		SelectComboBoxItem(ComboBox_Chapter);
		SelectComboBoxItem(ComboBox_Stage);
	}

	private void OnDungeonSelect(int value)
	{
		UpdateComboBox(SelectedItemType.Type);
	}

	private void OnChapterSelect(int value)
	{
		UpdateComboBox(SelectedItemType.Chapter);
	}

	private void OnStageSelect(int value)
	{
		UpdateComboBox(SelectedItemType.Stage);
	}

	private bool IsEnterAbleRoom(RaidRoomInfo roomInfo)
	{
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanGoToChapter(roomInfo.ChapterIndex) && GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(roomInfo.ChapterIndex, roomInfo.DungeonIndex);
		DungeonDifficultyType maxAvailableDifficulty = (DungeonDifficultyType)Helper.GetMaxAvailableDifficulty(roomInfo.ChapterIndex, roomInfo.DungeonIndex);
		if (roomInfo.DungeonDifficulty == PartyDungeonDifficultyType.Easy)
		{
			flag = flag && DungeonDifficultyType.Easy <= maxAvailableDifficulty;
		}
		else if (roomInfo.DungeonDifficulty == PartyDungeonDifficultyType.Normal)
		{
			flag = flag && DungeonDifficultyType.Normal <= maxAvailableDifficulty;
		}
		else if (roomInfo.DungeonDifficulty == PartyDungeonDifficultyType.Hard)
		{
			flag = flag && DungeonDifficultyType.Hard <= maxAvailableDifficulty;
		}
		else if (roomInfo.DungeonDifficulty == PartyDungeonDifficultyType.Hell)
		{
			flag = flag && DungeonDifficultyType.Hell <= maxAvailableDifficulty;
		}
		return flag;
	}
}
