using System;
using System.Linq;
using MiniJSON;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NMessageLog;
using NGame2.NNotify;
using NGame2.NParty;
using NGame2.NUI;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NMatch;

public class RematchManager : SingletonAuto<RematchManager>, DuelWaitingPopup.Listener
{
	private static MatchedAccountInfo _RematchOpponentInfo;

	public RematchManager()
	{
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<InviteRematchReq>
		{
			Handler = Handle
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<InviteRematchRes>
		{
			Handler = Handle
		});
	}

	public bool TryOpenRematchPopup(MatchResultInfo matchResultInfo)
	{
		if (SingletonUpdater<MatchManager>.instance.DuelType != MatchDuelType.Arena || matchResultInfo.ArenaType != MatchArenaType.Normal || matchResultInfo.Win || matchResultInfo.GiveUp || matchResultInfo.Canceled)
		{
			_RematchOpponentInfo = null;
			return false;
		}
		long currentStamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(StaminaType.Sword);
		int num = 0;
		if (currentStamina <= num)
		{
			return false;
		}
		IDataProvider dataProvider = SingletonAuto<BattleContext>.instance.DataProvider;
		if (dataProvider == null)
		{
			VespaDebug.LogError("DataProvider is null while opening rematch popup.");
			return false;
		}
		if (dataProvider.IsOnline())
		{
			ArenaContext context = dataProvider as ArenaContext;
			MatchedAccountInfo myInfo = context.MatchInfo.AccountInfos.FirstOrDefault((MatchedAccountInfo e) => e.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id);
			MatchedAccountInfo opponentInfo = context.MatchInfo.AccountInfos.FirstOrDefault((MatchedAccountInfo e) => e.AccountId != GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id);
			if (myInfo == null || opponentInfo == null)
			{
				return false;
			}
			if (_RematchOpponentInfo != null && opponentInfo.AccountId == _RematchOpponentInfo.AccountId)
			{
				_RematchOpponentInfo = null;
				return false;
			}
			_RematchOpponentInfo = null;
			MessagePopup3Button.Open_BattlePopup(LocalizationManager.GetStringDirect("REMATCH"), string.Format(LocalizationManager.GetStringDirect("REMATCH_INVITE_MESSAGE", opponentInfo.Nick)), delegate
			{
				BaseRewardPopup baseRewardPopup = GlobalAutoBehaviour<PopupManegement>.instance.FindWindow<BaseRewardPopup>();
				if (baseRewardPopup != null && baseRewardPopup.IsValid)
				{
					baseRewardPopup.Close();
				}
			}, delegate
			{
				_RematchOpponentInfo = opponentInfo;
				GlobalAutoBehaviour<PartyManager>.instance.SendToMember(_RematchOpponentInfo.AccountId, new InviteRematchReq
				{
					BattleType = dataProvider.GetBattleType(),
					Nick = myInfo.Nick,
					TeamLevel = myInfo.UserInfo.TeamLevel,
					AvatarIndex = myInfo.AvatarHeroIndex,
					LastMatchInfo = context.MatchInfo
				});
				WindowPrefab<DuelWaitingPopup> windowPrefab = new WindowPrefab<DuelWaitingPopup>("Resources/Prefab2/Window/Match/DuelWaitingPopup", GameUIRoot.ELayerType.UI);
				windowPrefab.instance.Open(DuelWaitingPopup.EWaitingType.RequestDuel, this, 10);
				GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
			}, NShared.NLocalization.Common.COMMON_NO, NShared.NLocalization.Common.COMMON_YES, 10);
		}
		else
		{
			OfflineArenaContext context2 = dataProvider as OfflineArenaContext;
			MessagePopup3Button.Open_BattlePopup(LocalizationManager.GetStringDirect("REMATCH"), string.Format(LocalizationManager.GetStringDirect("REMATCH_INVITE_MESSAGE", context2.NpcNick)), delegate
			{
				BaseRewardPopup baseRewardPopup = GlobalAutoBehaviour<PopupManegement>.instance.FindWindow<BaseRewardPopup>();
				if (baseRewardPopup != null && baseRewardPopup.IsValid)
				{
					baseRewardPopup.Close();
				}
			}, delegate
			{
				SingletonUpdater<MatchManager>.instance.InitVsLoadingPlayer(context2.MyMatchInfo, isLeft: true, isOnline: true);
				SingletonUpdater<MatchManager>.instance.RegisterRematch_Pvp(MatchPvpType.Normal, context2.MyMatchInfo.AccountId, context2.MyMatchInfo, context2.MatchedNpcInfo, isNpc: true);
			}, NShared.NLocalization.Common.COMMON_NO, NShared.NLocalization.Common.COMMON_YES);
		}
		return true;
	}

	void DuelWaitingPopup.Listener.OnCancel()
	{
	}

	private void SendToMember<T>(long receiverId, T data, Action onSuccess = null, Action onFail = null) where T : class
	{
		SingletonAuto<MessageLogHandler>.instance.SendToClient(new long[1] { receiverId }, data, onSuccess, onFail);
	}

	private void Handle(long senderId, InviteRematchReq data)
	{
		InviteRematchRes res = new InviteRematchRes
		{
			Result = InviteRematchRes.ResultType.CannotAccept
		};
		if (!InviteNotifyManager.IsOpenableRematch(senderId))
		{
			SendToMember(senderId, res);
			return;
		}
		long currentStamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(StaminaType.Sword);
		int num = 0;
		if (currentStamina <= num)
		{
			SendToMember(senderId, res);
			return;
		}
		MatchInfo matchInfo = SingletonAuto<ArenaContext>.instance.MatchInfo;
		if (matchInfo == null || data.LastMatchInfo == null)
		{
			SendToMember(senderId, res);
		}
		else if (Json.Serialize(matchInfo) != Json.Serialize(data.LastMatchInfo))
		{
			VespaDebug.LogError("MatchInfo mismatch Mine: " + Json.Serialize(matchInfo));
			VespaDebug.LogError("MatchInfo mismatch Their: " + Json.Serialize(data.LastMatchInfo));
			SendToMember(senderId, res);
		}
		else if (((data.LastMatchInfo == null) ? null : data.LastMatchInfo.AccountInfos.FirstOrDefault((MatchedAccountInfo e) => e.AccountId != GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id)) == null)
		{
			SendToMember(senderId, res);
		}
		else if (SingletonUpdater<MatchManager>.instance.IsWaitingMatchPvp)
		{
			SingletonUpdater<MatchManager>.instance.TryPauseMatching(delegate
			{
				AskAcceptRematch(senderId, data);
			}, delegate
			{
				SendToMember(senderId, res);
			});
		}
		else
		{
			AskAcceptRematch(senderId, data, wasWaitingMatch: false);
		}
	}

	private void AskAcceptRematch(long senderId, InviteRematchReq data, bool wasWaitingMatch = true)
	{
		InviteRematchRes res = new InviteRematchRes
		{
			Result = InviteRematchRes.ResultType.CannotAccept
		};
		MatchedAccountInfo myInfo = ((data.LastMatchInfo == null) ? null : data.LastMatchInfo.AccountInfos.FirstOrDefault((MatchedAccountInfo e) => e.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id));
		MatchedAccountInfo opponentInfo = ((data.LastMatchInfo == null) ? null : data.LastMatchInfo.AccountInfos.FirstOrDefault((MatchedAccountInfo e) => e.AccountId != GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id));
		GlobalAutoBehaviour<InviteNotifyManager>.instance.AddNotify(InviteNotifyType.Rematch, data, delegate(bool accept)
		{
			if (accept)
			{
				res.Result = InviteRematchRes.ResultType.Accepted;
				res.MatchInfo = data.LastMatchInfo;
				SendToMember(senderId, res, delegate
				{
					SingletonUpdater<MatchManager>.instance.RegisterRematch_Pvp(MatchPvpType.Normal, senderId, myInfo, opponentInfo);
				}, delegate
				{
					CenterMessage.Open(InviteRematchRes.ResultType.Rejected);
					SingletonUpdater<MatchManager>.instance.ResumeWaitingMatchIfPaused();
				});
			}
			else
			{
				res.Result = InviteRematchRes.ResultType.Rejected;
				SendToMember(senderId, res);
				SingletonUpdater<MatchManager>.instance.ResumeWaitingMatchIfPaused();
			}
		}, delegate
		{
			SingletonAuto<MessageLogHandler>.instance.SendToClient(senderId, res);
			SingletonUpdater<MatchManager>.instance.ResumeWaitingMatchIfPaused();
		});
	}

	private void Handle(long senderId, InviteRematchRes data)
	{
		GlobalAutoBehaviour<PopupManegement>.instance.CloseAll();
		if (data.Result == InviteRematchRes.ResultType.Accepted)
		{
			MatchedAccountInfo matchedAccountInfo = ((data.MatchInfo == null) ? null : data.MatchInfo.AccountInfos.FirstOrDefault((MatchedAccountInfo e) => e.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id));
			MatchedAccountInfo matchedAccountInfo2 = ((data.MatchInfo == null) ? null : data.MatchInfo.AccountInfos.FirstOrDefault((MatchedAccountInfo e) => e.AccountId != GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id));
			if (matchedAccountInfo == null || matchedAccountInfo2 == null)
			{
				GlobalMessagePopup.OpenGlobalError(BaseResultType.AccountError);
			}
			else
			{
				SingletonUpdater<MatchManager>.instance.RegisterRematch_Pvp(MatchPvpType.Normal, matchedAccountInfo.AccountId, matchedAccountInfo, matchedAccountInfo2);
			}
		}
		else if (data.Result == InviteRematchRes.ResultType.Rejected)
		{
			GlobalMessagePopup.OpenGlobalInform(Duel.DUEL_REJECT);
		}
		else
		{
			GlobalMessagePopup.OpenGlobalInform(Duel.DUEL_RESPONSE_BUSY);
		}
	}
}
