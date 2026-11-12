using System;
using System.Collections.Generic;
using NGame2.NParty;
using NGame2.NUI.NManager.NLobby;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NShared.NParty;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class RaidLevelSelect : BaseWindow
{
	public interface Listener
	{
		void OnSelectRaidLevel(RaidLevelSelect caller, int raidIndex, int level);

		void OnSelectRaidLevelForSingleRaid(RaidLevelSelect caller, int raidIndex, int level);

		void OnChangeSingleRoomSetting(RaidLevelSelect caller, RaidData raidData);

		void OnChangeRoomSetting(RaidLevelSelect caller, int raidLevel, RaidRoomType type, bool isViolateBanRule);
	}

	public enum WindowType
	{
		Create,
		Setting
	}

	public enum ChallengeDiffType
	{
		None,
		Easy,
		Normal,
		Hard,
		Hell
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Raid/RaidLevelSelect";

	private ChallengeDiffType _currChallengeDiff = ChallengeDiffType.Easy;

	public UITexture PortraitTexture;

	public UILabel TitleLabel;

	public UILabel NameLabel;

	public UILabel LevelLabel;

	public UIWidget LevelRootWidget;

	public UILabel HardModeLabel;

	public UILabel ChallengeDiffLebel;

	public UIButton PrevButton;

	public UIButton NextButton;

	public GameObject HelpButtonRoot;

	public GameObject RaidDescRoot;

	public UILabel RaidDescLabel;

	public Transform RaidTitle;

	public Transform ChallengeRaidTitle;

	public Transform Btn_Yes_Raid;

	public UILabel Btn_Label_Raid;

	public Transform Btn_Yes_ChallengeRaid;

	public UILabel Btn_Label_ChallengeRaid;

	public UIToggle PublicRoomToggle;

	public UIComboBox RoomTypeComboBox;

	public Transform ButtonSingleMultiRoot;

	public UIButton ButtonSingleRaid;

	public UILabel LabelSingleRaid;

	public UILabel LabelMultiRaid;

	public UIWidget ToggleSinglePartyRoot;

	public UIWidget PartyMenuGroupRoot;

	public UIToggle ToggleParty;

	public UIToggle ToggleSingle;

	public UIGrid Grid;

	private Listener _listener;

	private int _raidIndex;

	private int _raidLevel;

	private int _minRaidLevel;

	private int _maxRaidLevel;

	private WindowType _windowType;

	private bool _isParty = true;

	private bool _isSingleRoom;

	private RaidData _raidData;

	private bool _initComboBox;

	private Action OnPrevLevel;

	private Action OnNextLevel;

	public RaidData RaidData => _raidData;

	public bool IsPublic => PublicRoomToggle.value;

	public RaidRoomType RoomType => (RaidRoomType)RoomTypeComboBox.CurrentKey;

	private bool CheckIsLevelStepInfo
	{
		get
		{
			if (_raidData == null)
			{
				return false;
			}
			if (_raidData.Type == RaidType.DragonNormal || _raidData.Type == RaidType.DragonHard || _raidData.Type == RaidType.DragonNormalSingle || _raidData.Type == RaidType.DragonHardSingle || _raidData.Type == RaidType.FieldRaid || _raidData.Type == RaidType.FieldRaidSingle || _raidData.Type == RaidType.TechnoMagic || _raidData.Type == RaidType.TechnoMagicSingle || _raidData.Type == RaidType.TechnoEnchant || _raidData.Type == RaidType.TechnoEnchantSingle)
			{
				return true;
			}
			return false;
		}
	}

	private void SetRoomTypeComboBox()
	{
		if (_initComboBox)
		{
			return;
		}
		_initComboBox = true;
		foreach (RaidRoomType value in Enum.GetValues(typeof(RaidRoomType)))
		{
			if (value != RaidRoomType.All)
			{
				string raidRoomTypeString = RaidManagement.GetRaidRoomTypeString(value, _raidData, isTitle: false);
				RoomTypeComboBox.AddItem((int)value, raidRoomTypeString);
			}
		}
		RoomTypeComboBox.Reposition();
	}

	public void Open(Listener listener, int raidIndex, int raidLevel, WindowType windowType = WindowType.Create, bool isSingleRoom = false)
	{
		_raidData = GameRoot.Instance.RaidDataContainer.GetData(raidIndex, raidLevel);
		if (_raidData == null)
		{
			VespaDebug.LogError("RaidLevelSelect::Init / RaidData not found : raidIndex=" + _raidIndex + " raidLevel=" + _raidLevel);
			return;
		}
		base.Open();
		_isParty = true;
		_isSingleRoom = isSingleRoom;
		_listener = listener;
		_raidIndex = raidIndex;
		_raidLevel = raidLevel;
		_minRaidLevel = GameRoot.Instance.RaidDataContainer.GetRaidMinLevel(raidIndex);
		_maxRaidLevel = RaidHelper.GetRaidMaxLevel(raidIndex);
		_windowType = windowType;
		_currChallengeDiff = GetChallengeDiff(_raidLevel);
		SetRoomTypeComboBox();
		RoomTypeComboBox.SelectFirstItem();
		PublicRoomToggle.Set(state: true);
		Init();
	}

	public override void Reset()
	{
		base.Reset();
		ClosePortrait();
		_isParty = true;
		_isSingleRoom = false;
	}

	private void Init()
	{
		if (_raidData.Type == RaidType.Challenge)
		{
			OnPrevLevel = OnClickPrevOnChallenge;
			OnNextLevel = OnClickNextOnChallenge;
		}
		else
		{
			OnPrevLevel = OnClickPrevOnRaid;
			OnNextLevel = OnClickNextOnRaid;
		}
		InitTitle();
		UpdateLevel();
		InitPortrait(_raidData);
		InitComboBox();
		InitDeco();
		SetPartyToggle();
		UIHelper.EnableGameObject(HelpButtonRoot, _raidData.Type != RaidType.Challenge);
		bool flag = !String_Extension_Method.IsNullOrEmpty(_raidData.RaidDesc);
		UIHelper.EnableGameObject(RaidDescRoot, flag);
		if (flag)
		{
			string stringDirect = LocalizationManager.GetStringDirect(_raidData.RaidDesc);
			UIHelper.SetLabelText(RaidDescLabel, stringDirect);
		}
		if (_windowType == WindowType.Setting)
		{
			PartyInfo partyInfo = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo;
			bool flag2 = false;
			if (partyInfo != null)
			{
				flag2 = partyInfo.IsPublicRoom();
				PublicRoomToggle.Set(flag2);
				if (partyInfo.PartyRoom != null)
				{
					byte goalIndex = partyInfo.PartyRoom.GoalIndex;
					RoomTypeComboBox.SelectItem(goalIndex, isNotify: false);
				}
			}
			UIHelper.SetWidgetGrayScale(ToggleSinglePartyRoot, grayScale: true, setColliders: true);
			UIHelper.SetWidgetGrayScale(PublicRoomToggle.GetComponent<UIWidget>(), grayScale: true, setColliders: true);
			UIHelper.EnableGameObject(ButtonSingleMultiRoot, enable: false);
			UIHelper.EnableGameObject(Btn_Yes_Raid, enable: true);
			UIHelper.SetWidgetGrayScale(RoomTypeComboBox.GetComponent<UIWidget>(), !flag2, setColliders: true);
			UIHelper.EnableCollider(RoomTypeComboBox, flag2);
			string stringDirect2 = LocalizationManager.GetStringDirect("RAID_SETTING");
			UIHelper.SetLabelText((_raidData.Type == RaidType.Challenge) ? Btn_Label_ChallengeRaid : Btn_Label_Raid, stringDirect2);
		}
		else
		{
			UIHelper.SetWidgetGrayScale(ToggleSinglePartyRoot, grayScale: false, setColliders: true);
			UIHelper.EnableGameObject(ToggleSinglePartyRoot, _raidData.Type != RaidType.Challenge);
			string stringDirect3 = LocalizationManager.GetStringDirect("RAID_ENTER");
			UIHelper.SetLabelText((_raidData.Type == RaidType.Challenge) ? Btn_Label_ChallengeRaid : Btn_Label_Raid, stringDirect3);
			stringDirect3 = LocalizationManager.GetStringDirect("RAID_ENTER");
			UIHelper.SetLabelText(LabelMultiRaid, stringDirect3);
			stringDirect3 = LocalizationManager.GetStringDirect("RAID_CREATE_SINGLE_BUTTON");
			UIHelper.SetLabelText(LabelSingleRaid, stringDirect3);
			UIHelper.SetWidgetGrayScale(RoomTypeComboBox.GetComponent<UIWidget>(), grayScale: false, setColliders: true);
			UIHelper.EnableCollider(RoomTypeComboBox, isEnable: true);
			UIHelper.SetWidgetGrayScale(PublicRoomToggle.GetComponent<UIWidget>(), grayScale: false, setColliders: true);
			SetPartyMenuGroup(_isParty);
		}
		bool enable = _windowType == WindowType.Create;
		UIHelper.EnableToggle(PublicRoomToggle, enable, grayscale: true);
		if ((bool)PublicRoomToggle)
		{
			PublicRoomToggle.gameObject.SetActive(value: false);
			PublicRoomToggle.gameObject.SetActive(value: true);
		}
		UIHelper.EnableButton(ButtonSingleRaid, _raidData.SingleRaidIndex != 0);
		Grid.Reposition();
	}

	public void SetPartyToggle()
	{
		if (_isSingleRoom)
		{
			_isParty = false;
		}
		ToggleParty.value = _isParty;
		ToggleSingle.value = !_isParty;
	}

	public void SetPartyMenuGroup(bool enable)
	{
		UIHelper.SetWidgetGrayScale(PartyMenuGroupRoot, !enable, setColliders: true);
		UIHelper.EnableGameObject(PartyMenuGroupRoot, enable: false);
		UIHelper.EnableGameObject(PartyMenuGroupRoot, enable: true);
		UIHelper.EnableCollider(RoomTypeComboBox, enable);
	}

	private void InitTitle()
	{
		string stringDirect = LocalizationManager.GetStringDirect((_raidData.Type == RaidType.Challenge) ? "SELECT_DIFFICULTY" : "GRADE_TITLE");
		UIHelper.SetLabelText(TitleLabel, stringDirect);
	}

	private void InitDeco()
	{
		UIHelper.EnableGameObject(Btn_Yes_ChallengeRaid, _raidData.Type == RaidType.Challenge);
		UIHelper.EnableGameObject(ChallengeRaidTitle, _raidData.Type == RaidType.Challenge);
		UIHelper.EnableGameObject(Btn_Yes_Raid, _raidData.Type != RaidType.Challenge && _raidData.SingleRaidIndex == 0);
		UIHelper.EnableGameObject(ButtonSingleMultiRoot, _raidData.Type != RaidType.Challenge && _raidData.SingleRaidIndex != 0);
		UIHelper.EnableGameObject(RaidTitle, _raidData.Type != RaidType.Challenge);
	}

	private void UpdateLevel()
	{
		RaidData data = GameRoot.Instance.RaidDataContainer.GetData(_raidIndex, _raidLevel);
		if (data == null)
		{
			VespaDebug.LogError("RaidData not found : raidIndex=" + _raidIndex + " raidLevel=" + _raidLevel);
			return;
		}
		string stringDirect = LocalizationManager.GetStringDirect(data.Name);
		UIHelper.SetLabelText(NameLabel, stringDirect);
		if (CheckIsLevelStepInfo)
		{
			string stringDirect2 = LocalizationManager.GetStringDirect(data.Step_Name);
			UIHelper.SetLabelText(LevelLabel, stringDirect2);
		}
		else if (data.Type == RaidType.Challenge)
		{
			UIHelper.SetLabelText(ChallengeDiffLebel, _currChallengeDiff.ToString());
		}
		UpdateLabelState(data.Type);
		UpdateButtonState(data.Type);
	}

	private void UpdateLabelState(RaidType raidType)
	{
		UIHelper.EnableGameObject(LevelRootWidget, CheckIsLevelStepInfo);
		UIHelper.EnableGameObject(ChallengeDiffLebel, raidType == RaidType.Challenge);
	}

	private void UpdateButtonState(RaidType raidType)
	{
		if (CheckIsLevelStepInfo)
		{
			UIHelper.EnableButton(PrevButton, _raidLevel > _minRaidLevel, grayscaleIfDisable: true);
			UIHelper.EnableButton(NextButton, _raidLevel < _maxRaidLevel, grayscaleIfDisable: true);
		}
		else if (raidType == RaidType.Challenge)
		{
			RaidHelper.GetRaidMaxLevel(_raidIndex);
			UIHelper.EnableButton(PrevButton, _currChallengeDiff > ChallengeDiffType.Easy, grayscaleIfDisable: true);
			UIHelper.EnableButton(NextButton, _currChallengeDiff < GetChallengeDiff(_maxRaidLevel), grayscaleIfDisable: true);
		}
	}

	private void InitPortrait(RaidData raidData)
	{
		ClosePortrait();
		RaidHelper.InitPortrait(this, PortraitTexture, raidData);
	}

	private void InitComboBox()
	{
		UIHelper.EnableGameObject(RoomTypeComboBox, _raidData.Type != RaidType.Challenge);
	}

	private void ClosePortrait()
	{
		UIHelper.ClearPlaneAssetTexture(PortraitTexture);
	}

	public void OnClickSelectLevelForSingleRaid()
	{
		if (_listener != null)
		{
			_listener.OnSelectRaidLevelForSingleRaid(this, _raidIndex, _raidLevel);
		}
	}

	public void OnClickSelectLevel()
	{
		if (_raidData.Type == RaidType.Challenge)
		{
			_isParty = true;
		}
		if (_listener == null)
		{
			return;
		}
		if (_windowType == WindowType.Create)
		{
			if (_isParty)
			{
				_listener.OnSelectRaidLevel(this, _raidIndex, _raidLevel);
			}
			else
			{
				OnClickSelectLevelForSingleRaid();
			}
			return;
		}
		RaidRoomType currentKey = (RaidRoomType)RoomTypeComboBox.CurrentKey;
		if (_isSingleRoom)
		{
			RaidData data = GameRoot.Instance.RaidDataContainer.GetData(_raidIndex, _raidLevel);
			if (data.SingleRaidIndex > 0)
			{
				RaidData data2 = GameRoot.Instance.RaidDataContainer.GetData(data.SingleRaidIndex, _raidLevel);
				_listener.OnChangeSingleRoomSetting(this, data2);
			}
			return;
		}
		bool isViolateBanRule = false;
		RaidData data3 = GameRoot.Instance.RaidDataContainer.GetData(_raidIndex, _raidLevel);
		BanRuleData data4 = GameRoot.Instance.BanRuleDataContainer.GetData(data3.BanIndex);
		if (data4 != null)
		{
			RaidBanRuleData[] banRuleDataAry = data4.GetBanRuleDataAry();
			Tuple<List<CreatureTagType>, int> tuple = null;
			for (int i = 0; i < banRuleDataAry.Length; i++)
			{
				if (banRuleDataAry[i].BanType == RaidBanType.ClassEnterPermission)
				{
					tuple = RaidHelper.GetClassEnterBanRuleData(banRuleDataAry[i]);
					if (tuple != null)
					{
						break;
					}
				}
			}
			if (tuple != null)
			{
				RaidPartySetting instance = SceneAutoBehaviour<RaidManagement>.instance.GetInstance<RaidPartySetting>();
				if (instance != null)
				{
					RaidPartySetting.ClassEnterPermissionBanTypeData classEnterPermissionBanTypeData = new RaidPartySetting.ClassEnterPermissionBanTypeData();
					classEnterPermissionBanTypeData.LimitEnterClassList = tuple.Item1;
					classEnterPermissionBanTypeData.PossibleEnterCount = tuple.Item2;
					classEnterPermissionBanTypeData.RuleTactic = data4.RuleTactic;
					isViolateBanRule = instance.IsViolateBanRule(classEnterPermissionBanTypeData);
				}
			}
		}
		_listener.OnChangeRoomSetting(this, _raidLevel, currentKey, isViolateBanRule);
	}

	private ChallengeDiffType GetChallengeDiff(int levelIndex)
	{
		return (ChallengeDiffType)System.Math.Min(System.Math.Max(levelIndex, 1), 4);
	}

	private void OnClickPrevOnChallenge()
	{
		if (_currChallengeDiff != ChallengeDiffType.Easy)
		{
			_currChallengeDiff--;
			_raidLevel = (int)_currChallengeDiff;
			UpdateLevel();
		}
	}

	private void OnClickNextOnChallenge()
	{
		if (_currChallengeDiff != GetChallengeDiff(_maxRaidLevel))
		{
			_currChallengeDiff++;
			_raidLevel = (int)_currChallengeDiff;
			UpdateLevel();
		}
	}

	private void OnClickPrevOnRaid()
	{
		if (_raidLevel > _minRaidLevel)
		{
			_raidLevel--;
			UpdateLevel();
		}
	}

	private void OnClickNextOnRaid()
	{
		if (_raidLevel < _maxRaidLevel)
		{
			_raidLevel++;
			UpdateLevel();
		}
	}

	public void OnClickPrevLevel()
	{
		OnPrevLevel();
	}

	public void OnClickNextLevel()
	{
		OnNextLevel();
	}

	public void OnTogglePublicRoom()
	{
	}

	public void OnToggleSingle()
	{
		if (_isParty && ToggleSingle.value)
		{
			_isParty = false;
			SetPartyMenuGroup(_isParty);
		}
	}

	public void OnToggleParty()
	{
		if (!_isParty && ToggleParty.value)
		{
			_isParty = true;
			SetPartyMenuGroup(_isParty);
		}
	}

	public void OnClickGuideButton()
	{
		string stringDirect = LocalizationManager.GetStringDirect(GameRoot.Instance.RaidDataContainer.GetData(_raidIndex, _raidLevel).Step_HelpDesc);
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.GUIDE_MENU);
		string message = stringDirect;
		MessagePopup.Open(text, message, null, null, NShared.NLocalization.Common.COMMON_CONFIRM, NShared.NLocalization.Common.COMMON_CANCEL);
	}
}
