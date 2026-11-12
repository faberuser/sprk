using System;
using System.Collections.Generic;
using NGame2.NBattleContext;
using NGame2.NBattleField;
using NGame2.NChatting.NUserBlock;
using NGame2.NMatch;
using NGame2.NParty;
using NGame2.NScene;
using NGame2.NTutorial;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NAttendance.NIntegration;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI3.NAttendance;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NNotify;

public class InviteNotifyManager : GlobalAutoBehaviour<InviteNotifyManager>
{
	private InviteNotyfiData CurrentInviteNotify;

	public InviteNotifyWindow CurrentWindow;

	private Stack<InviteNotyfiData> InviteNotifyStack = new Stack<InviteNotyfiData>();

	public void AddNotify(InviteNotifyType type, object data, Action<bool> onAccept, Action onFailed = null)
	{
		InviteNotyfiData inviteNotyfiData = new InviteNotyfiData
		{
			Type = type,
			Data = data,
			OnAccept = onAccept,
			OnFailed = onFailed
		};
		if (CurrentInviteNotify != null)
		{
			if (CurrentWindow != null)
			{
				CurrentInviteNotify.WindowOpenedTime = CurrentWindow.OpenedTime;
				CurrentWindow.Close();
			}
			InviteNotifyStack.Push(CurrentInviteNotify);
		}
		CurrentInviteNotify = inviteNotyfiData;
		OpenInviteNotify(inviteNotyfiData);
	}

	public void OpenInviteNotify(InviteNotyfiData notify)
	{
		CurrentInviteNotify = notify;
		if (notify == null)
		{
			GetNextInviteNotify();
			return;
		}
		Action<bool> onAction = delegate(bool isAccept)
		{
			if (notify.OnAccept != null)
			{
				notify.OnAccept(isAccept);
			}
			if (isAccept)
			{
				ClearList();
			}
			else
			{
				GetNextInviteNotify();
			}
		};
		Action onFailed = delegate
		{
			if (notify.OnFailed != null)
			{
				notify.OnFailed();
			}
			GetNextInviteNotify();
		};
		if (notify.Type == InviteNotifyType.Raid)
		{
			OpenRaidInviteNotify(notify.Data as InvitePartyReq, onAction);
		}
		else if (notify.Type == InviteNotifyType.Duel)
		{
			OpenDuelInviteNotify(notify.Data as RequestDuel, onAction, onFailed);
		}
		else if (notify.Type == InviteNotifyType.Rematch)
		{
			OpenRematchNotify(notify.Data as InviteRematchReq, onAction, onFailed);
		}
		if (CurrentWindow != null && notify.WindowOpenedTime != 0f)
		{
			CurrentWindow.SetOpenedTime(notify.WindowOpenedTime);
		}
	}

	private void OpenRaidInviteNotify(InvitePartyReq data, Action<bool> onAction)
	{
		if (data == null)
		{
			GetNextInviteNotify();
		}
		else
		{
			CurrentWindow = RaidInviteNoty.Open(data, onAction);
		}
	}

	private void OpenDuelInviteNotify(RequestDuel data, Action<bool> onAction, Action onFailed)
	{
		if (data == null)
		{
			GetNextInviteNotify();
		}
		else
		{
			CurrentWindow = DuelRequestNotify.Open(data, onAction, onFailed);
		}
	}

	private void OpenRematchNotify(InviteRematchReq data, Action<bool> onAction, Action onFailed)
	{
		if (data == null)
		{
			GetNextInviteNotify();
			return;
		}
		RequestDuel data2 = new RequestDuel
		{
			Nick = data.Nick,
			AvatarIndex = data.AvatarIndex,
			TeamLevel = data.TeamLevel
		};
		CurrentWindow = DuelRequestNotify.Open(data2, onAction, onFailed);
	}

	private void ClearList()
	{
		CurrentInviteNotify = null;
		CurrentWindow = null;
		InviteNotifyStack.Clear();
	}

	private void GetNextInviteNotify()
	{
		CurrentInviteNotify = null;
		CurrentWindow = null;
		if (InviteNotifyStack.Count != 0)
		{
			InviteNotyfiData notify = InviteNotifyStack.Pop();
			OpenInviteNotify(notify);
		}
	}

	public static bool IsOpenableRematch(long senderId)
	{
		if (SingletonUpdater<TutorialManager>.isValidInstance && SingletonUpdater<TutorialManager>.instance.IsProgressing)
		{
			return false;
		}
		if (SingletonAuto<UserBlockManager>.instance.IsBlockedUser(senderId))
		{
			return false;
		}
		if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Login)
		{
			return false;
		}
		if (PvpLoading.isValidInstance && PvpLoading.instance != null && PvpLoading.instance.IsMatched)
		{
			return false;
		}
		if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Battle2 && !SingletonAuto<BattleFacade>.instance.GetBattleCache().IsBattleEnded)
		{
			return false;
		}
		return true;
	}

	public static bool IsOpenableNoty(long senderId)
	{
		if (SingletonUpdater<TutorialManager>.isValidInstance && SingletonUpdater<TutorialManager>.instance.IsProgressing)
		{
			return false;
		}
		if (SingletonAuto<UserBlockManager>.instance.IsBlockedUser(senderId))
		{
			return false;
		}
		if (SingletonAuto<BattleContext>.isValidInstance && GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Battle2)
		{
			return false;
		}
		if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Login)
		{
			return false;
		}
		if (PvpLoading.isValidInstance && PvpLoading.instance != null && PvpLoading.instance.IsOpen())
		{
			return false;
		}
		if (SceneAutoBehaviour<MatchManagement>.isValidInstance && SceneAutoBehaviour<MatchManagement>.instance.IsOpenedBanPickWindow())
		{
			return false;
		}
		if (SceneAutoBehaviour<SoulWeaponManagement>.isValidInstance && SceneAutoBehaviour<SoulWeaponManagement>.instance.IsOpened<EclipseMain>())
		{
			return false;
		}
		if (SceneAutoBehaviour<SoulWeaponManagement>.isValidInstance && (SceneAutoBehaviour<SoulWeaponManagement>.instance.IsOpened<ContentUIGodkingTrialDungeonInfo>() || SceneAutoBehaviour<SoulWeaponManagement>.instance.IsOpened<ContentUIGodkingTrialDungeonList>()))
		{
			return false;
		}
		if (SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			if (SceneAutoBehaviour<LobbyManagement>.instance.IsOpened<AttendanceIntegrationWindow>())
			{
				return false;
			}
			if (SingletonAuto<AttendanceManager>.instance.IsOpened_Any())
			{
				return false;
			}
		}
		if (SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.isValidInstance && SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.instance.IsOpened<ShakmehDungeonWindow>())
		{
			return false;
		}
		if (SceneAutoBehaviour<MultipleDungeonManagement>.isValidInstance && SceneAutoBehaviour<MultipleDungeonManagement>.instance.IsOpened<MultipleDungeonWindow>())
		{
			return false;
		}
		if (SingletonUpdater<DuelManager>.instance.IsPlayDuelMatch())
		{
			return false;
		}
		if (GlobalAutoBehaviour<PartyManager>.instance.IsValidParty())
		{
			return false;
		}
		if (GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpened<RaidSinglePartySetting>())
		{
			return false;
		}
		if (SceneAutoBehaviour<BlacksmithManagement>.instance.GetInstance<HeroEquipAwaken>().IsOpen())
		{
			return false;
		}
		if (GlobalAutoBehaviour<PopupManegement>.instance.FindWindow<OrdealArenaBuffSelectPopup>() != null)
		{
			return false;
		}
		if (SceneAutoBehaviour<KingsPassManagement>.isValidInstance && SceneAutoBehaviour<KingsPassManagement>.instance.IsOpenKingsPassPopup())
		{
			return false;
		}
		if ((SceneAutoBehaviour<RaidManagement>.isValidInstance && SceneAutoBehaviour<RaidManagement>.instance.IsOpened<PunishmentRaidRewardInfoPopup>()) || SceneAutoBehaviour<RaidManagement>.instance.IsOpened<RaidInfo>())
		{
			return false;
		}
		return true;
	}
}
