using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NChatting;
using NGame2.NChatting.NUserBlock;
using NGame2.NContentsDefine;
using NGame2.NMatch;
using NGame2.NParty;
using NGame2.NScene;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.Mission;
using NShared;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NShared.NParty;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class InspectUserMenuPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/InspectUserMenuPopup";

	public UIBundleLoader AvatarBundleTex;

	public UILabel LabelName;

	public UISimpleGrid ButtonGrid;

	public UIWidget ResizeGroup;

	public UIButton UserInfoButton;

	public UIButton WhisperButton;

	public UIButton FriendRequestButton;

	public UIWidget FriendAlreadyInviteWidget;

	public UIButton FriendDeleteButton;

	public UIButton BlockButton;

	public UIButton UnblockButton;

	public UILabel InviteLabel;

	public UIButton InviteRaidButton;

	public UILabel JoinLabel;

	public UIButton JoinRaidButton;

	public UIButton BanRaidButton;

	public UIButton BanPartyButton;

	public UIButton NewMasterButton;

	public UIButton DuelButton;

	private InspectUserManagement.InspectUserData _inspectUser;

	public static void Open(InspectUserManagement.InspectUserData inspectUser)
	{
		GameUIRoot.ELayerType layerType = GameUIRoot.ELayerType.UI;
		if (GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpenedUI3DUI())
		{
			layerType = GameUIRoot.ELayerType.UI3DUI;
		}
		WindowPrefab<InspectUserMenuPopup> windowPrefab = new WindowPrefab<InspectUserMenuPopup>("Resources/Prefab2/Window/MessagePopup/InspectUserMenuPopup", layerType);
		windowPrefab.instance.OpenInternal(inspectUser);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void OpenInternal(InspectUserManagement.InspectUserData inspectUser)
	{
		base.Open();
		_inspectUser = inspectUser;
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.FriendManager.IsFriend(inspectUser.AccountId);
		bool flag2 = GlobalAutoBehaviour<AccountManager>.instance.FriendManager.IsInvitee(inspectUser.AccountId);
		bool flag3 = CanInviteRaid();
		string iconPath = UIAvatarHelper.GetIconPath(inspectUser.AvatarHeroIndex);
		bool flag4 = SingletonAuto<UserBlockManager>.instance.IsBlockedUser(inspectUser.AccountId);
		bool enable = GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Lobby;
		UIHelper.SetBundleTexture(AvatarBundleTex, iconPath);
		string text = inspectUser.Nickname;
		if (GuiManager.IsShowDebugButton())
		{
			text = text + " (" + inspectUser.AccountId + ")";
		}
		UIHelper.SetLabelText(LabelName, text);
		UIHelper.EnableGameObject(UserInfoButton, enable);
		UIHelper.EnableGameObject(WhisperButton, enable: true);
		bool enable2 = !flag && flag2;
		bool enable3 = !flag && !flag2;
		UIHelper.EnableGameObject(FriendDeleteButton, flag);
		UIHelper.EnableGameObject(FriendAlreadyInviteWidget, enable2);
		UIHelper.EnableGameObject(FriendRequestButton, enable3);
		UIHelper.EnableGameObject(BlockButton, !flag4);
		UIHelper.EnableGameObject(UnblockButton, flag4);
		string key = ((flag3 && GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.IsRaid()) ? "INVITE_RAID_BUTTON" : "FRIEND_INVITE_PARTY");
		UIHelper.SetLabelText(InviteLabel, LocalizationManager.GetStringDirect(key));
		UIHelper.EnableGameObject(InviteRaidButton, flag3);
		UIHelper.EnableGameObject(JoinRaidButton, enable: false);
		bool flag5 = CanBanFromRaidParty();
		UIHelper.EnableGameObject(BanRaidButton, flag5 && _inspectUser.Category == InspectUserCategory.Raid);
		UIHelper.EnableGameObject(BanPartyButton, flag5 && _inspectUser.Category == InspectUserCategory.Party);
		UIHelper.EnableGameObject(NewMasterButton, flag5);
		SprkSetDuelButtonVisible(flag);
		UIHelper.EnableButton(DuelButton, enable: false, grayscaleIfDisable: true);
		RequestDungeonInfo();
		RequestCanDuel();
		SprkHonorDuel.SetupAndRefresh(this);
	}

	private void RefreshButton()
	{
		UIHelper.RepositionGrid(ButtonGrid);
		if (ButtonGrid != null && ButtonGrid.Grid != null && ResizeGroup != null)
		{
			int gridChildCount = ButtonGrid.Grid.GetGridChildCount();
			ResizeGroup.height = Mathf.RoundToInt(ButtonGrid.Grid.cellHeight * (float)Mathf.Max(1, gridChildCount));
		}
	}

	private void RequestDungeonInfo()
	{
		if (GlobalAutoBehaviour<PartyManager>.instance.IsValidParty() || (SceneAutoBehaviour<MatchManagement>.isValidInstance && SceneAutoBehaviour<MatchManagement>.instance.IsOpenedBanPickWindow()) || (!PartyManager.IsOpenedRaid() && !PartyManager.IsOpenedPartyDungeon()))
		{
			return;
		}
		if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Battle2)
		{
			IDataProvider dataProvider = SingletonAuto<BattleContext>.instance.DataProvider;
			if ((dataProvider != null && dataProvider.GetBattleType() == BattleType.TreasureHouse) || (dataProvider != null && dataProvider.GetBattleType() == BattleType.Eclipse))
			{
				return;
			}
		}
		GlobalAutoBehaviour<PartyManager>.instance.SendPartyMemberDungeonInfoReq(_inspectUser.AccountId, delegate(long senderId, PartyMemberDungeonInfoRes info)
		{
			if (senderId == _inspectUser.AccountId && info.CanJoin)
			{
				bool enable = true;
				bool enable2 = true;
				bool flag = false;
				if (info.RaidIndex != 0 && info.RaidLevel != 0)
				{
					RaidData data = GameRoot.Instance.RaidDataContainer.GetData(info.RaidIndex, info.RaidLevel);
					if (data == null)
					{
						return;
					}
					if (data.Type == RaidType.DragonHard)
					{
						enable2 = RaidHelper.IsOpenedHardMode(data);
					}
					else if (data.Type == RaidType.TechnoMagic)
					{
						enable2 = RaidHelper.IsOpenedTechnoMagicRaid(data);
					}
					else if (data.Type == RaidType.GuildSuppress)
					{
						enable = false;
					}
					flag = true;
				}
				else if (info.ChapterIndex != 0 && info.DungeonIndex != 0)
				{
					if (GameRoot.Instance.CampaignDungeonDataContainer.GetData(info.ChapterIndex, info.DungeonIndex) == null)
					{
						VespaDebug.LogError("DungeonData found :  chapter=" + info.ChapterIndex + " dungeon=" + info.DungeonIndex);
						return;
					}
					enable2 = true;
				}
				if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Battle2)
				{
					IDataProvider dataProvider2 = SingletonAuto<BattleContext>.instance.DataProvider;
					if (dataProvider2 != null && dataProvider2.GetBattleType() == BattleType.GuildRaid)
					{
						enable2 = false;
					}
				}
				_inspectUser.SetRoomNo(info.RoomNo);
				string stringDirect = LocalizationManager.GetStringDirect(flag ? "JOIN_RAID_BUTTON" : "PARTY_JOIN");
				UIHelper.SetLabelText(JoinLabel, stringDirect);
				UIHelper.EnableGameObject(JoinRaidButton, enable);
				UIHelper.EnableButton(JoinRaidButton, enable2);
				RefreshButton();
			}
		});
	}

	private void RequestCanDuel()
	{
		if ((!GlobalAutoBehaviour<PartyManager>.isValidInstance || !GlobalAutoBehaviour<PartyManager>.instance.IsValidParty()) && SingletonUpdater<DuelManager>.instance.CanDuel(_inspectUser.AccountId, isMyRequest: true))
		{
			SingletonUpdater<DuelManager>.instance.SendCanDuel(_inspectUser.AccountId, delegate(long senderId, CanDuelResponse response)
			{
				UIHelper.EnableButton(DuelButton, response.CanDuel, grayscaleIfDisable: true);
				SprkHonorDuel.SetupAndRefresh(this);
			});
		}
	}

	private bool CanBanFromRaidParty()
	{
		if (!GlobalAutoBehaviour<PartyManager>.isValidInstance || !GlobalAutoBehaviour<PartyManager>.instance.IsValidParty())
		{
			return false;
		}
		MemberInfo partyMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetPartyMemberInfo(GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId);
		if (partyMemberInfo == null || !partyMemberInfo.IsMaster)
		{
			return false;
		}
		MemberInfo partyMemberInfo2 = GlobalAutoBehaviour<PartyManager>.instance.GetPartyMemberInfo(_inspectUser.AccountId);
		if (partyMemberInfo2 == null || partyMemberInfo2.IsMaster)
		{
			return false;
		}
		if (GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.IsBattleStarted())
		{
			return false;
		}
		return true;
	}

	private bool CanInviteRaid()
	{
		if (GlobalAutoBehaviour<PartyManager>.instance.GetRaidType() == RaidType.GuildSuppress)
		{
			return false;
		}
		if (!GlobalAutoBehaviour<PartyManager>.instance.IsValidParty())
		{
			return false;
		}
		if (!GlobalAutoBehaviour<PartyManager>.instance.CanInviteSomeone())
		{
			return false;
		}
		if (GlobalAutoBehaviour<PartyManager>.instance.GetPartyMemberInfo(_inspectUser.AccountId) != null)
		{
			return false;
		}
		return true;
	}

	public override void Reset()
	{
		base.Reset();
	}

	public override void Close()
	{
		base.Close();
		if (GlobalAutoBehaviour<PartyManager>.isValidInstance)
		{
			GlobalAutoBehaviour<PartyManager>.instance.ClearPartyMemberDungeonInfoReq();
		}
	}

	public void OnClickButton(UIButton button)
	{
		bool flag = false;
		if (button == UserInfoButton)
		{
			SceneAutoBehaviour<InspectUserManagement>.instance.RequestInspectUser(_inspectUser);
		}
		else if (button == WhisperButton)
		{
			GlobalAutoBehaviour<ChattingManager>.instance.AddWhisperUser(_inspectUser.AccountId, _inspectUser.Nickname);
			SceneAutoBehaviour<ChattingManagement>.instance.OpenChatting(ChatType.Whisper, _inspectUser.AccountId);
		}
		else if (button == FriendRequestButton)
		{
			SceneAutoBehaviour<MyFriendManagement>.instance.ProcessInviteFriend(_inspectUser.AccountId, _inspectUser.Nickname);
		}
		else if (button == FriendDeleteButton)
		{
			SceneAutoBehaviour<MyFriendManagement>.instance.ProcessDeleteFriend(_inspectUser.AccountId, _inspectUser.Nickname);
		}
		else if (button == BlockButton)
		{
			if (SingletonAuto<UserBlockManager>.instance.CanBlockMore())
			{
				long accountId = _inspectUser.AccountId;
				string userName = _inspectUser.Nickname;
				string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
				string message = LocalizationManager.GetString(GameOption.BLOCK_USER_MESSAGE, userName);
				MessagePopup.Open(text, message, delegate
				{
					SingletonAuto<UserBlockManager>.instance.BlockUser(accountId, userName);
					CenterMessage.Open(LocalizationManager.GetString(GameOption.BLOCK_USER_MESSAGE_CENTER, userName));
				}, null);
			}
			else
			{
				CenterMessage.Open(LocalizationManager.GetString(GameOption.CANNOT_BLOCK_MORE_USER));
			}
		}
		else if (button == UnblockButton)
		{
			long accountId2 = _inspectUser.AccountId;
			string userName2 = _inspectUser.Nickname;
			string text2 = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message2 = LocalizationManager.GetString(GameOption.UNBLOCK_USER_MESSAGE, userName2);
			MessagePopup.Open(text2, message2, delegate
			{
				SingletonAuto<UserBlockManager>.instance.UnblockUser(accountId2);
				CenterMessage.Open(LocalizationManager.GetString(GameOption.UNBLOCK_USER_MESSAGE_CENTER, userName2));
			}, null);
		}
		else if (button == InviteRaidButton)
		{
			GlobalAutoBehaviour<PartyManager>.instance.SendInvitePartyRequest(_inspectUser.AccountId, _inspectUser.Nickname);
		}
		else if (button == JoinRaidButton)
		{
			flag = true;
			CurrencyMaxCap.WarningOrExecute(CurrencyType.RaidPoint, delegate
			{
				long roomNo = _inspectUser.RoomNo;
				GlobalAutoBehaviour<PartyManager>.instance.SendJoinPartyRequest_Accept(_inspectUser.AccountId, roomNo);
			});
		}
		else if (button == BanRaidButton)
		{
			string nickname = _inspectUser.Nickname;
			string text3 = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message3 = LocalizationManager.GetString(Party.PARTY_ASK_BAN_USER, nickname);
			MessagePopup.Open(text3, message3, delegate
			{
				GlobalAutoBehaviour<PartyManager>.instance.BanUser(_inspectUser.AccountId);
			}, null);
		}
		else if (button == BanPartyButton)
		{
			string nickname2 = _inspectUser.Nickname;
			string text4 = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message4 = LocalizationManager.GetString(Party.PARTY_ASK_BAN_USER_2, nickname2);
			MessagePopup.Open(text4, message4, delegate
			{
				GlobalAutoBehaviour<PartyManager>.instance.BanUser(_inspectUser.AccountId);
			}, null);
		}
		else if (button == DuelButton)
		{
			long id = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
			SingletonUpdater<DuelManager>.instance.RequestDuel(_inspectUser.AccountId, id);
		}
		else if (button == NewMasterButton)
		{
			GlobalAutoBehaviour<PartyManager>.instance.SendNewPartyRoomMaster(_inspectUser.AccountId);
		}
		else
		{
			VespaDebug.LogError("Not Implemented Yet: " + Vespa_Unity_Object_Extension_Method.NAME(button));
		}
		if (flag && SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager != null)
		{
			SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.CloseMissionWnds();
		}
		Close();
	}

	public void SprkSetDuelButtonVisible(bool isFriend)
	{
		UIHelper.EnableGameObject(DuelButton, isFriend);
	}
}
