using System;
using NGame2.NContentsDefine;
using NGame2.NUI.NHelper.NAffix;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.NSoulWeapon;
using NShared;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class RaidInviteNoty : InviteNotifyWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Raid/RaidInviteNoty";

	private static GlobalWindowPrefab<RaidInviteNoty> _windowPrefab;

	public UIBundleLoader PortaritBundle;

	public UITexture PortraitTexture;

	public UILabel NameLabel;

	public UILabel LevelLabel;

	public UILabel MessageLabel;

	public UILabel PointLabel;

	public UIWidget MonsterLevelRoot;

	public UILabel HardModeLabel;

	public UILabel ChallengeModeLabel;

	public UILabelAligner LabelAligner;

	public UIButton BtnYes;

	public UISprite SprRaidDeco;

	private RaidData _invitedRaidData;

	private PartyDungeonData _invitedPartyDungeonData;

	public static RaidInviteNoty Open(InvitePartyReq data, Action<bool> onAction)
	{
		if (_windowPrefab == null)
		{
			_windowPrefab = new GlobalWindowPrefab<RaidInviteNoty>("Resources/Prefab2/Window/Raid/RaidInviteNoty", GlobalUIRoot.ELayerType.GlobalUI);
		}
		_windowPrefab.instance.OpenInternal(data);
		_windowPrefab.instance.SetCallBack(onAction);
		return _windowPrefab.instance;
	}

	private void OpenInternal(InvitePartyReq data)
	{
		OpenInternal();
		if (data.RaidIndex != 0 && data.RaidLevel != 0)
		{
			OpenRaid(data);
		}
		else if (data.chapterIndex != 0 && data.dungeonIndex != 0)
		{
			OpenParty(data);
		}
		if ((bool)LabelAligner)
		{
			LabelAligner.Align();
		}
		CheckSceneLoading();
	}

	private void OpenRaid(InvitePartyReq data)
	{
		_invitedRaidData = GameRoot.Instance.RaidDataContainer.GetData(data.RaidIndex, data.RaidLevel);
		if (_invitedRaidData == null)
		{
			VespaDebug.LogError("RaidData not found : raidIndex=" + data.RaidIndex + " raidLevel=" + data.RaidLevel);
			return;
		}
		string stringDirect = LocalizationManager.GetStringDirect(_invitedRaidData.Name);
		string stringDirect2 = LocalizationManager.GetStringDirect(_invitedRaidData.Step_Name);
		bool enable = RaidHelper.CanEnterRaid(_invitedRaidData);
		string text = LocalizationManager.GetString(Party.PARTY_INVITE_FROM, data.InvitorName);
		UIHelper.EnableButton(BtnYes, enable);
		RaidHelper.InitPortrait(this, PortraitTexture, _invitedRaidData);
		UIHelper.SetLabelText(NameLabel, stringDirect);
		UIHelper.SetLabelText(MessageLabel, text);
		if (_invitedRaidData.Type == RaidType.DragonNormal || _invitedRaidData.Type == RaidType.DragonHard || _invitedRaidData.Type == RaidType.FieldRaid || _invitedRaidData.Type == RaidType.TechnoEnchant)
		{
			UIHelper.SetLabelText(LevelLabel, stringDirect2);
		}
		else if (_invitedRaidData.Type == RaidType.Challenge)
		{
			int affixPoint = GetAffixPoint(data.AffixIndices);
			UIHelper.SetLabelText(ChallengeModeLabel, RaidHelper.ChallengeRaidDiffClamp(data.RaidLevel).ToString());
			UIHelper.SetLabelText(PointLabel, $"{affixPoint}P");
		}
		UIHelper.EnableGameObject(MonsterLevelRoot, _invitedRaidData.Type == RaidType.DragonNormal || _invitedRaidData.Type == RaidType.DragonHard || _invitedRaidData.Type == RaidType.FieldRaid || _invitedRaidData.Type == RaidType.TechnoEnchant);
		UIHelper.EnableGameObject(HardModeLabel, enable: false);
		UIHelper.EnableGameObject(ChallengeModeLabel, _invitedRaidData.Type == RaidType.Challenge);
		UIHelper.EnableGameObject(PointLabel, _invitedRaidData.Type == RaidType.Challenge);
		UIHelper.EnableGameObject(SprRaidDeco, _invitedRaidData.Type != RaidType.GuildSuppress);
	}

	private void OpenParty(InvitePartyReq data)
	{
		_invitedPartyDungeonData = GameRoot.Instance.PartyDungeonDataContainer.GetData(data.chapterIndex, data.dungeonIndex);
		if (_invitedPartyDungeonData == null)
		{
			VespaDebug.LogError("PartyDungeonData not found : chapterIndex=" + data.chapterIndex + " dungeonIndex=" + data.dungeonIndex);
			return;
		}
		_ = data.dungeonType;
		_ = data.difficulty;
		int chapterIndex = _invitedPartyDungeonData.ChapterIndex;
		int dungeonIndex = _invitedPartyDungeonData.DungeonIndex;
		CampaignNodeData data2 = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
		string empty = string.Empty;
		empty = ((!String_Extension_Method.IsNullOrEmpty(data2.PortalNameCode)) ? LocalizationManager.GetStringDirect(data2.PortalNameCode) : data2.Name);
		string stringDirect = LocalizationManager.GetStringDirect(empty);
		string text = LocalizationManager.GetString(Party.PARTY_INVITE_FROM, data.InvitorName);
		UIHelper.EnableButton(BtnYes, enable: true);
		UIHelper.SetBundleTexture(PortaritBundle, _invitedPartyDungeonData.PortraitPath);
		UIHelper.SetLabelText(NameLabel, stringDirect);
		UIHelper.SetLabelText(MessageLabel, text);
		UIHelper.EnableGameObject(ChallengeModeLabel, enable: false);
		UIHelper.EnableGameObject(MonsterLevelRoot, enable: false);
		UIHelper.EnableGameObject(HardModeLabel, enable: false);
		UIHelper.EnableGameObject(SprRaidDeco, enable: false);
	}

	private int GetAffixPoint(int[] affixIndices)
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(_invitedRaidData.ChapterIndex, _invitedRaidData.DungeonIndex);
		if (data == null)
		{
			return 0;
		}
		NShared.DungeonDifficultyType challengeDungeonDifficulty = RaidHelper.GetChallengeDungeonDifficulty();
		return AffixHelper.GetTotalAffixPoint(data, challengeDungeonDifficulty, affixIndices);
	}

	private NShared.DungeonDifficultyType GetChallengeDifficulty(int raidLevel)
	{
		return (NShared.DungeonDifficultyType)System.Math.Min(System.Math.Max(raidLevel - 1, 0), 3);
	}

	public override void OnClickAccept()
	{
		if (!SceneAutoBehaviour<MultiPartyManagement>.instance.CheckEnterBattle())
		{
			return;
		}
		if (SceneAutoBehaviour<SoulWeaponManagement>.isValidInstance)
		{
			SoulStoneRestoreWindow instance = SceneAutoBehaviour<SoulWeaponManagement>.instance.GetInstance<SoulStoneRestoreWindow>();
			if (instance != null && instance.IsSoulStoneSelect())
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("SOULWEAPON_RESTORE_BUFF_POPUP"));
				return;
			}
		}
		base.OnClickAccept();
		CurrencyType currencyType = CurrencyType.None;
		if (_invitedRaidData != null)
		{
			currencyType = ContentsDefine.CurrencyMaxCap.GetCurrencyTypeOnEndBattle(_invitedRaidData.Type);
		}
		CurrencyMaxCap.WarningOrExecute(currencyType, delegate
		{
			Accept_Real();
		});
	}

	private void Accept_Real()
	{
		if (_invitedRaidData != null && !RaidHelper.CanEnterRaid(_invitedRaidData))
		{
			OnClickDeny();
			return;
		}
		if (_invitedPartyDungeonData != null && !RaidHelper.CanEnterParty(_invitedPartyDungeonData))
		{
			OnClickDeny();
			return;
		}
		Action<bool> onAction = OnAction;
		_windowPrefab.ReleaseWindow();
		Vespa_System_Action_Extension_Method.fire(onAction, i_argA: true);
	}

	public override void OnClickDeny()
	{
		Action<bool> onAction = OnAction;
		_windowPrefab.ReleaseWindow();
		Vespa_System_Action_Extension_Method.fire(onAction, i_argA: false);
	}
}
