using NGame2.NAccount;
using NGame2.NDispatchBattle;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public sealed class RaidListComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate, IUserObserver<UserDispatchBattleInfoManager.Observer.BroadCastInfo>, IUserObserver
{
	public interface Listener
	{
		void OnClickEnter(RaidListComponent caller);

		void OnClickViewInfo(RaidListComponent caller);

		void OnClickRaidRoomList(RaidListComponent caller);

		void OnClickDispatch(RaidListComponent caller);
	}

	public const string PrefabPath = "Prefab2/Component/Raid/RaidListComponent";

	public static PrefabAuto<RaidListComponent> PrefabAuto = new PrefabAuto<RaidListComponent>("Prefab2/Component/Raid/RaidListComponent");

	public UIButton EnterRaidButton;

	public UIBundleLoader PortraitBundleTex;

	public UILabel NameLabel;

	public UILabel LevelLabel;

	public UIButton CreateRaidButton;

	public UIButton RaidListButton;

	public GameObject MonsterLabelRoot;

	public GameObject HardModeLabelRoot;

	public UISprite NormalModeBG;

	public UISprite HardModeBG;

	public UISprite FieldRaidBG;

	public UIButton InfoButton;

	public UILabel InfoButtonLabel;

	public UIWidget LockRoot;

	public UILabel LockLabel;

	public UIButton ButtonDispatch;

	public UIButton ButtonDispatchFinished;

	private Listener _listener;

	private int _raidIndex;

	private RaidData _raidData;

	private bool _isActiveRaid;

	public RaidData RaidData => _raidData;

	public int RaidIndex => _raidIndex;

	private bool CheckIsDragonRaid
	{
		get
		{
			if (_raidData == null)
			{
				return false;
			}
			if (_raidData.Type == RaidType.FieldRaid || _raidData.Type == RaidType.TechnoMagic)
			{
				return false;
			}
			return true;
		}
	}

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager != null)
		{
			GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.ObserverManager.AddObserver(this);
		}
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager != null)
		{
			GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.ObserverManager.AddObserver(this);
		}
		ClosePortrait();
	}

	public void Open(int raidIndex, Listener listener)
	{
		_raidIndex = raidIndex;
		_listener = listener;
		Init(raidIndex);
	}

	public void Init(int raidIndex)
	{
		int raidMaxLevel = RaidHelper.GetRaidMaxLevel(raidIndex);
		RaidData data = GameRoot.Instance.RaidDataContainer.GetData(_raidIndex, raidMaxLevel);
		if (data == null)
		{
			VespaDebug.LogError("RaidData not found : raidIndex=" + _raidIndex + " raidLevel=" + raidMaxLevel);
			return;
		}
		string stringDirect = LocalizationManager.GetStringDirect(data.Name);
		_raidData = data;
		string stringDirect2 = LocalizationManager.GetStringDirect(_raidData.Step_Name);
		UIHelper.SetLabelText(LevelLabel, stringDirect2);
		UIHelper.SetLabelText(NameLabel, stringDirect);
		string key = ((data.Type == RaidType.FieldRaid) ? "FIELDRAID_DUNGEON_REWARD_INFO" : "RAID_INFO");
		UIHelper.SetLabelText(InfoButtonLabel, LocalizationManager.GetStringDirect(key));
		bool enable = data.Type == RaidType.DragonNormal || data.Type == RaidType.TechnoMagic || data.Type == RaidType.TechnoEnchant;
		UIHelper.EnableGameObject(NormalModeBG, enable);
		UIHelper.EnableGameObject(HardModeBG, data.Type == RaidType.DragonHard);
		UIHelper.EnableGameObject(FieldRaidBG, data.Type == RaidType.FieldRaid);
		bool flag = IsRaidClear(data);
		InitPortrait();
		InitState(flag);
		UpdateButtonDispatch(flag);
		InitFieldRaidLockState(!flag);
	}

	private void InitPortrait()
	{
		RaidHelper.InitPortrait(this, PortraitBundleTex, _raidData);
	}

	private void ClosePortrait()
	{
		UIHelper.SetBundleTexture(PortraitBundleTex, string.Empty);
	}

	private void InitState(bool isActive)
	{
		_isActiveRaid = isActive;
		UIHelper.EnableButton(CreateRaidButton, isActive);
		UIHelper.EnableButton(RaidListButton, isActive);
		bool enable = CheckIsDragonRaid || isActive;
		UIHelper.EnableButton(InfoButton, enable);
	}

	private void InitFieldRaidLockState(bool isLock)
	{
		if (CheckIsDragonRaid || !isLock)
		{
			UIHelper.EnableGameObject(LockRoot, enable: false);
			return;
		}
		string text = "";
		if (RaidData.Level == 1)
		{
			string text2 = RaidHelper.FieldRaidOpenNode(_raidData);
			if (!String_Extension_Method.IsNullOrEmpty(text2))
			{
				text = LocalizationManager.GetStringDirect("FIELD_RAID_OPEN_CONDITION", text2);
			}
		}
		UIHelper.SetLabelText(LockLabel, text);
		UIHelper.EnableGameObject(LockRoot, enable: true);
	}

	private void UpdateButtonDispatch(bool isRaidActive)
	{
		UIHelper.EnableButton(ButtonDispatch, enable: false, grayscaleIfDisable: true);
		UIHelper.EnableGameObject(ButtonDispatch, enable: true);
		UIHelper.EnableGameObject(ButtonDispatchFinished, enable: false);
		if (_raidData != null && GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager != null)
		{
			if (!isRaidActive || !IsDispatchable())
			{
				UIHelper.EnableButton(ButtonDispatch, enable: false, grayscaleIfDisable: true);
				UIHelper.EnableGameObject(ButtonDispatch, enable: true);
				UIHelper.EnableGameObject(ButtonDispatchFinished, enable: false);
			}
			else if (IsExistCompleteDispatch())
			{
				UIHelper.EnableGameObject(ButtonDispatch, enable: false);
				UIHelper.EnableGameObject(ButtonDispatchFinished, enable: true);
			}
			else
			{
				UIHelper.EnableButton(ButtonDispatch, enable: true, grayscaleIfDisable: true);
				UIHelper.EnableGameObject(ButtonDispatch, enable: true);
				UIHelper.EnableGameObject(ButtonDispatchFinished, enable: false);
			}
		}
	}

	private bool IsDispatchable()
	{
		if (_raidData == null)
		{
			return false;
		}
		if (_raidData.Type == RaidType.TechnoEnchant || _raidData.Type == RaidType.TechnoEnchantSingle)
		{
			return false;
		}
		int raidMaxCompleteLv = DispatchHelper.GetRaidMaxCompleteLv(_raidData);
		int raidMinLevel = GameRoot.Instance.RaidDataContainer.GetRaidMinLevel(_raidData.Index);
		if (raidMaxCompleteLv < raidMinLevel)
		{
			return false;
		}
		return true;
	}

	private bool IsExistCompleteDispatch()
	{
		return DispatchHelper.IsExistCompleteDispatch(_raidData);
	}

	private bool IsRaidClear(RaidData raidData)
	{
		if (raidData == null)
		{
			return false;
		}
		bool result = true;
		switch (raidData.Type)
		{
		case RaidType.DragonHard:
			result = RaidHelper.CanEnterHardMode();
			break;
		case RaidType.FieldRaid:
			result = RaidHelper.IsOpenedFieldRaid(raidData);
			break;
		case RaidType.TechnoMagic:
			result = RaidHelper.IsOpenedTechnoMagicRaid(raidData);
			break;
		}
		return result;
	}

	public void OnClickEnter()
	{
		if (SceneAutoBehaviour<MultiPartyManagement>.instance.CheckEnterBattle() && _isActiveRaid && _listener != null)
		{
			_listener.OnClickEnter(this);
		}
	}

	public void OnClickViewInfo()
	{
		if (_listener != null)
		{
			_listener.OnClickViewInfo(this);
		}
	}

	public void OnClickRaidRoomList()
	{
		if (_listener != null)
		{
			_listener.OnClickRaidRoomList(this);
		}
	}

	public void OnClickDispatch()
	{
		if (_listener != null)
		{
			_listener.OnClickDispatch(this);
		}
	}

	void IUserObserver<UserDispatchBattleInfoManager.Observer.BroadCastInfo>.OnChanged(UserDispatchBattleInfoManager.Observer.BroadCastInfo changedInfo)
	{
		UserDispatchBattleInfoManager.Observer.BroadCastReason reason = changedInfo.Reason;
		if (reason == UserDispatchBattleInfoManager.Observer.BroadCastReason.OnUpdate || reason == UserDispatchBattleInfoManager.Observer.BroadCastReason.OnRemove)
		{
			UpdateButtonDispatch(IsRaidClear(_raidData));
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}
}
