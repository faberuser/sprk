using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager.NLobby;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class RaidRoomList : BaseWindow, RaidRoomComponent.Listener, LobbyTopmostMenu.IUserStatObserver
{
	public interface Listener
	{
		void OnClickEnterRaid(RaidRoomInfo roomInfo);

		void RefreshList(int raidIndex, int filterValue, RaidRoomType type);

		void OpenAffixSelectSetting(int raidindex, int raidLevel, int[] affixIndices);
	}

	private enum SortType
	{
		UP,
		Down
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Raid/RaidRoomList";

	private int RefreshTime = 5;

	public Transform RaidDeco;

	public Transform ChallengeRaidDeco;

	public UILabel TitleLabel;

	public UILabel RefreshTimeLabel;

	public UIButton RefreshButton;

	public UIComboBox LevelSortComboBox;

	public UIComboBox LevelFilterComboBox;

	public UIComboBox TypeFilterComboBox;

	public UIScrollGrid RoomGrid;

	public int[] NormalRaidFilterValues;

	private List<RaidRoomComponent> Components = new List<RaidRoomComponent>();

	private List<ChallengeRaidRoomComponent> ChallengeRaidRoomComponents = new List<ChallengeRaidRoomComponent>();

	private RaidRoomInfo[] RoomInfos;

	private Listener _Listener;

	private RaidData _raidData;

	private SortType LevelSortType;

	private int RaidIndex;

	public UserManager.UserStatType StatType
	{
		get
		{
			if (_raidData != null && (_raidData.Type == RaidType.DragonNormal || _raidData.Type == RaidType.DragonHard))
			{
				return UserManager.UserStatType.RaidPoint;
			}
			return UserManager.UserStatType.Gold;
		}
	}

	private bool CheckRaidType
	{
		get
		{
			if (_raidData == null)
			{
				return false;
			}
			if (_raidData.Type != RaidType.FieldRaid && _raidData.Type != RaidType.TechnoMagic)
			{
				return _raidData.Type == RaidType.TechnoEnchant;
			}
			return true;
		}
	}

	public override void Awake()
	{
		base.Awake();
		if (RoomGrid != null && RoomGrid.Grid != null)
		{
			RoomGrid.Grid.onCustomSort = CompareGrid;
		}
		RefreshTime = GameRoot.Instance.ConstantDataContainer.GetValueInt("RaidRoomListRefreshDelayTime");
	}

	private string GetSortName(SortType type)
	{
		return type switch
		{
			SortType.UP => LocalizationManager.GetString(NShared.NLocalization.Common.ASCENDING_ORDER), 
			SortType.Down => LocalizationManager.GetString(NShared.NLocalization.Common.DESCENDING_ORDER), 
			_ => "", 
		};
	}

	private void InitComboBox()
	{
		LevelSortComboBox.Clear();
		LevelSortComboBox.OnChanged = OnClickLevelSort;
		foreach (SortType value in Enum.GetValues(typeof(SortType)))
		{
			string sortName = GetSortName(value);
			LevelSortComboBox.AddItem((int)value, sortName);
		}
		LevelSortComboBox.Reposition();
		LevelFilterComboBox.Clear();
		SetLevelFilterComboBox();
		LevelFilterComboBox.Reposition();
		TypeFilterComboBox.Clear();
		foreach (RaidRoomType value2 in Enum.GetValues(typeof(RaidRoomType)))
		{
			string raidRoomTypeString = RaidManagement.GetRaidRoomTypeString(value2, _raidData, isTitle: false);
			TypeFilterComboBox.AddItem((int)value2, raidRoomTypeString);
		}
		TypeFilterComboBox.Reposition();
	}

	private void SetLevelFilterComboBox()
	{
		switch ((_raidData != null) ? _raidData.Type : RaidType.DragonNormal)
		{
		case RaidType.DragonHard:
		{
			string text2 = LocalizationManager.GetString(Raid.FILTER_RAID_GRADE_ALL);
			LevelFilterComboBox.AddItem(0, text2);
			return;
		}
		case RaidType.Challenge:
		{
			int num = 0;
			LevelFilterComboBox.AddItem(0, LocalizationManager.GetStringDirect("FILTER_RAID_DIFF_ALL"));
			ChallengeRaidDiff[] array = Enum.GetValues(typeof(ChallengeRaidDiff)) as ChallengeRaidDiff[];
			for (int i = 0; i < array.Length; i++)
			{
				ChallengeRaidDiff challengeRaidDiff = array[i];
				if (challengeRaidDiff != ChallengeRaidDiff.None)
				{
					num = RaidHelper.GetRaidBossLevel(_raidData.Index, (int)challengeRaidDiff);
					num -= num % 10;
					LevelFilterComboBox.AddItem(num, challengeRaidDiff.ToString());
				}
			}
			return;
		}
		case RaidType.DragonNormal:
		{
			int[] normalRaidFilterValues = NormalRaidFilterValues;
			foreach (int num2 in normalRaidFilterValues)
			{
				string text = "";
				text = ((num2 != 0) ? LocalizationManager.GetString(Raid.FILTER_RAID_GRADE_RANGE, num2) : LocalizationManager.GetString(Raid.FILTER_RAID_GRADE_ALL));
				LevelFilterComboBox.AddItem(num2, text);
			}
			return;
		}
		}
		if (!CheckRaidType)
		{
			return;
		}
		LevelFilterComboBox.AddItem(0, LocalizationManager.GetString(Raid.FILTER_RAID_GRADE_ALL));
		ICollection<int> raidLevels = GameRoot.Instance.RaidDataContainer.GetRaidLevels(_raidData.Index);
		string text3 = "";
		foreach (int item in raidLevels)
		{
			text3 = LocalizationManager.GetString(Raid.FILTER_RAID_GRADE_RANGE_FIELD, item);
			LevelFilterComboBox.AddItem(item, text3);
		}
	}

	public void Open(RaidRoomInfo[] roomInfos, int raidIndex, Listener listener)
	{
		int raidMinLevel = GameRoot.Instance.RaidDataContainer.GetRaidMinLevel(raidIndex);
		_raidData = GameRoot.Instance.RaidDataContainer.GetData(raidIndex, raidMinLevel);
		if (_raidData == null)
		{
			VespaDebug.LogError("RaidRoomList::Open raidData == null");
			return;
		}
		_Listener = listener;
		RaidIndex = raidIndex;
		RoomInfos = roomInfos;
		base.Open();
		Init();
	}

	public void Init()
	{
		InitComboBox();
		InitTitleDeco();
		CreateList(RoomInfos);
		LevelSortComboBox.SelectFirstItem(isNotify: false);
		LevelFilterComboBox.SelectFirstItem(isNotify: false);
		TypeFilterComboBox.SelectFirstItem(isNotify: false);
		ResetRefreshButton();
		UIHelper.EnableGameObject(TypeFilterComboBox, _raidData.Type != RaidType.Challenge);
	}

	private void InitTitleDeco()
	{
		UIHelper.EnableGameObject(ChallengeRaidDeco, _raidData.Type == RaidType.Challenge);
		UIHelper.EnableGameObject(RaidDeco, _raidData.Type != RaidType.Challenge);
		RaidType raidType = ((_raidData != null) ? _raidData.Type : RaidType.DragonNormal);
		string empty = string.Empty;
		UIHelper.SetLabelText(text: raidType switch
		{
			RaidType.DragonNormal => LocalizationManager.GetStringDirect("RAID_ROOM_LIST"), 
			RaidType.DragonHard => LocalizationManager.GetStringDirect("RAID_HARDMODE_ROOM_LIST"), 
			_ => LocalizationManager.GetStringDirect("RAID_ROOM_LIST"), 
		}, label: TitleLabel);
	}

	public override void Reset()
	{
		base.Reset();
		ClearList();
		LevelSortType = SortType.UP;
	}

	private void CreateList(RaidRoomInfo[] roomInfos)
	{
		if (roomInfos == null)
		{
			return;
		}
		bool flag = _raidData.Type == RaidType.Challenge;
		foreach (RaidRoomInfo roomInfo in roomInfos)
		{
			if (flag)
			{
				ChallengeRaidRoomComponent challengeRaidRoomComponent = UIHelper.AddGridItem(RoomGrid, ChallengeRaidRoomComponent.PrefabAuto);
				challengeRaidRoomComponent.Open(roomInfo, this);
				ChallengeRaidRoomComponents.Add(challengeRaidRoomComponent);
			}
			else
			{
				RaidRoomComponent raidRoomComponent = UIHelper.AddGridItem(RoomGrid, RaidRoomComponent.PrefabAuto);
				raidRoomComponent.Open(roomInfo, this);
				Components.Add(raidRoomComponent);
			}
		}
		UIHelper.RepositionGrid(RoomGrid);
	}

	private void ClearList()
	{
		FactoryBase.ReleaseAndClearList(Components);
		FactoryBase.ReleaseAndClearList(ChallengeRaidRoomComponents);
	}

	private int CompareGrid(Transform a, Transform b)
	{
		RaidRoomComponent component = a.GetComponent<RaidRoomComponent>();
		RaidRoomComponent component2 = b.GetComponent<RaidRoomComponent>();
		if (component == component2)
		{
			return 0;
		}
		if (component == null)
		{
			return 1;
		}
		if (component2 == null)
		{
			return -1;
		}
		int num = 0;
		if (LevelSortType == SortType.UP)
		{
			num = component.Level.CompareTo(component2.Level);
		}
		else if (LevelSortType == SortType.Down)
		{
			num = component2.Level.CompareTo(component.Level);
		}
		if (num == 0)
		{
			num = component2.Index.CompareTo(component.Index);
		}
		return num;
	}

	public void OnClickRefresh()
	{
		ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, CheckRefresh());
		ClearList();
		if (_Listener == null)
		{
			return;
		}
		int filterValue = 0;
		int currentKey = LevelFilterComboBox.CurrentKey;
		if (_raidData != null && CheckRaidType)
		{
			RaidData data = GameRoot.Instance.RaidDataContainer.GetData(_raidData.Index, currentKey);
			if (data != null)
			{
				filterValue = data.FilterValue;
			}
		}
		else
		{
			filterValue = currentKey;
		}
		RaidRoomType currentKey2 = (RaidRoomType)TypeFilterComboBox.CurrentKey;
		_Listener.RefreshList(RaidIndex, filterValue, currentKey2);
	}

	public void RefreshList(RaidRoomInfo[] roomInfos)
	{
		RoomInfos = roomInfos;
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

	void RaidRoomComponent.Listener.OnClickEnterRaid(RaidRoomInfo roomInfo)
	{
		if (_Listener != null)
		{
			_Listener.OnClickEnterRaid(roomInfo);
		}
	}

	void RaidRoomComponent.Listener.OpenAffixSelectSetting(int raidindex, int raidLevel, int[] affixIndices)
	{
		if (_Listener != null)
		{
			_Listener.OpenAffixSelectSetting(raidindex, raidLevel, affixIndices);
		}
	}

	private void OnClickLevelSort(int key)
	{
		LevelSortType = (SortType)key;
		UIHelper.RepositionGrid(RoomGrid);
	}
}
