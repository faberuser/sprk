using NGame2.NParty;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow.NAffix;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class AffixSelectPopup : BaseAffixPanel
{
	public new interface IListener : BaseAffixPanel.IListener
	{
		void OnClickCreateRoom(AffixSelectPopup caller);

		void OnClickMultiSettingRoom(AffixSelectPopup caller);

		void OnClickSingelSettingRoom(AffixSelectPopup caller);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/AffixSystem/AffixSelectPopup";

	private readonly string[] AFFIX_GUIDE = new string[8] { "GAME_GUIDE", "", "AFFIX_POPUP_INFO_NAME_1", "AFFIX_POPUP_INFO_DESC_1", "AFFIX_POPUP_INFO_NAME_2", "AFFIX_POPUP_INFO_DESC_2", "AFFIX_POPUP_INFO_NAME_3", "AFFIX_POPUP_INFO_DESC_3" };

	public UIToggle TogglePublicRoom;

	public GameObject TogglePublicRoomSelectRoot;

	private int _raidIndex;

	private int _raidLevel;

	private bool _canChanged;

	private bool _isParty;

	private bool _isPublic;

	private bool _isSingleRoom;

	private int _minRaidLevel;

	private int _maxRaidLevel;

	private DungeonDifficultyType _diffcultyType;

	private new IListener _listener;

	private RaidData _raidData;

	public bool IsPublic => _isPublic;

	public RaidRoomType RoomType => RaidRoomType.Help;

	public void OpenCreate(int raidIndex, int raidLevel, IListener listener)
	{
		Open(raidIndex, raidLevel, null, canChange: true, EAffixWindowType.Create, listener);
	}

	public void OpenSetting(int raidIndex, int raidLevel, int[] affixIndices, bool canChange, IListener listener)
	{
		Open(raidIndex, raidLevel, affixIndices, canChange, EAffixWindowType.Setting, listener);
	}

	public void OpenChallenge(int raidIndex, int raidLevel, int[] affixIndices, IListener listener)
	{
		Open(raidIndex, raidLevel, affixIndices, canChange: true, EAffixWindowType.Challenge, listener);
	}

	private void Open(int raidIndex, int raidLevel, int[] affixIndices, bool canChange, EAffixWindowType windowType, IListener listener)
	{
		Open(delegate
		{
			_listener = listener;
			Setup(raidIndex, raidLevel, affixIndices, canChange, windowType, windowOpen: true);
		}, delegate
		{
		});
	}

	protected override void Release()
	{
		base.Release();
		_raidData = null;
		_raidIndex = 0;
		_raidLevel = 0;
		_isParty = false;
		_isSingleRoom = false;
		if (AlwaysSaveToggle != null)
		{
			UIHelper.EnableToggle(AlwaysSaveToggle, enable: true, grayscale: true);
		}
		if (TogglePublicRoom != null)
		{
			UIHelper.EnableToggle(TogglePublicRoom, enable: true, grayscale: true);
		}
	}

	private void Setup(int raidIndex, int raidLevel, int[] affixIndices, bool canChange, EAffixWindowType windowType, bool windowOpen = false)
	{
		SetValues(raidIndex, raidLevel, canChange, windowType);
		SetContainer();
		LoadAffixIndices(affixIndices);
		if (windowOpen)
		{
			InitDifficultyComp();
			InitCategory();
			InitAlwaysSaveToggle();
			InitPublicRoomToggle();
			InitButtons();
		}
		SetSelectComponent();
		SetInfoComponent();
		SetInfoLabels();
	}

	protected override void SetValues(int raidIndex, int raidLevel, bool canChange, EAffixWindowType windowType)
	{
		_raidIndex = raidIndex;
		_raidLevel = raidLevel;
		_minRaidLevel = GameRoot.Instance.RaidDataContainer.GetRaidMinLevel(raidIndex);
		_maxRaidLevel = RaidHelper.GetRaidMaxLevel(raidIndex);
		_diffcultyType = GetDungeonDifficulty();
		_raidData = GameRoot.Instance.RaidDataContainer.GetData(raidIndex, raidLevel);
		if (_raidData == null)
		{
			VespaDebug.LogError($"Not Exist RaidData Index : {raidIndex} Level : {raidLevel}");
			return;
		}
		int chapterIndex = _raidData.ChapterIndex;
		int dungeonIndex = _raidData.DungeonIndex;
		base.SetValues(chapterIndex, dungeonIndex, canChange, windowType);
	}

	protected void InitPublicRoomToggle()
	{
		bool flag = IsMultiRaid();
		bool enable = _windowType != EAffixWindowType.Setting && flag && _canChange;
		bool defaultPublicRoom = GetDefaultPublicRoom();
		if (TogglePublicRoom != null)
		{
			TogglePublicRoom.Set(defaultPublicRoom);
			ShowToggleSelectRoot(TogglePublicRoom, TogglePublicRoomSelectRoot);
		}
		UIHelper.EnableToggle(TogglePublicRoom, enable, grayscale: true);
	}

	private bool GetDefaultPublicRoom()
	{
		bool flag = IsMultiRaid();
		switch (_windowType)
		{
		case EAffixWindowType.Create:
			return flag;
		case EAffixWindowType.Setting:
			if (flag)
			{
				if (GlobalAutoBehaviour<PartyManager>.isValidInstance && GlobalAutoBehaviour<PartyManager>.instance.PartyInfo != null && GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.PartyRoom != null)
				{
					return GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.IsPublicRoom();
				}
				return true;
			}
			return false;
		case EAffixWindowType.Challenge:
			return true;
		default:
			return true;
		}
	}

	protected override bool GetLocalStarogeAlwaysSave()
	{
		bool flag = IsMultiRaid();
		if (_windowType == EAffixWindowType.Setting && flag && (!GlobalAutoBehaviour<PartyManager>.isValidInstance || GlobalAutoBehaviour<PartyManager>.instance.PartyInfo == null || GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.PartyRoom == null))
		{
			return false;
		}
		return base.GetLocalStarogeAlwaysSave();
	}

	private bool IsMultiRaid()
	{
		if (_raidData == null)
		{
			return false;
		}
		return _raidData.Type == RaidType.Challenge;
	}

	protected override void InitDifficultyComp()
	{
		if (ComboBoxDifficulty == null || _initComboBoxDiffculty)
		{
			return;
		}
		_initComboBoxDiffculty = true;
		foreach (int raidLevel in GameRoot.Instance.RaidDataContainer.GetRaidLevels(_raidIndex))
		{
			ComboBoxDifficulty.AddItem(raidLevel - 1, "");
		}
		ComboBoxDifficulty.SetDisableClick(!_canChange);
		ComboBoxDifficulty.OnChanged = null;
		ComboBoxDifficulty.SelectItem(_raidLevel - 1);
		ComboBoxDifficulty.OnChanged = OnChangedDifficulty;
		ComboBoxDifficulty.Reposition();
	}

	protected override void OnChangedDifficulty(int difficulty)
	{
		int num = difficulty + 1;
		if (_raidLevel != num)
		{
			RepositionInfoScrollGrid();
			Setup(_raidIndex, num, null, _canChange, _windowType);
		}
	}

	protected override void OnClickSelectPopupComp(AffixSelectPopupComponent caller)
	{
		if (!(caller == null))
		{
			AffixData affixData = caller.GetAffixData();
			bool flag = _container.SelectContains(affixData);
			if (_container.CanSelect(flag, affixData))
			{
				_container.Select(affixData);
				RefreshSelectComponents();
				SetInfoComponent();
				SetInfoLabels();
			}
		}
	}

	public RaidData GetRaidData()
	{
		return _raidData;
	}

	public void OnClickCreateRoom()
	{
		if (_listener != null)
		{
			_listener.OnClickCreateRoom(this);
		}
	}

	public void OnClickSettingRoom()
	{
		if (_listener != null && _raidData != null)
		{
			if (_raidData.Type == RaidType.Challenge)
			{
				_listener.OnClickMultiSettingRoom(this);
			}
			else
			{
				_listener.OnClickSingelSettingRoom(this);
			}
		}
	}

	public void OnChangedPublicRoom(UIToggle toggle)
	{
		if (!(toggle == null))
		{
			ShowToggleSelectRoot(toggle, TogglePublicRoomSelectRoot);
			_isPublic = toggle.value;
		}
	}

	public void OnClickEventGuidePopup()
	{
		EventGuidePopup.Open(AFFIX_GUIDE);
	}
}
