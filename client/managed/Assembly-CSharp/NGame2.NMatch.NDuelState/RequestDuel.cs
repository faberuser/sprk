using NGame2.NAccount;
using NGame2.NMessageLog;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NMatch.NDuelState;

public class RequestDuel : StateBase
{
	private float _waitingPopupClosedTime;

	public RequestDuel(long vsPlayerId, long groupId)
		: base(vsPlayerId, groupId)
	{
	}

	public override void ChainUpdate()
	{
		base.ChainUpdate();
		if (!SceneAutoBehaviour<DuelManagement>.instance.IsOpened<DuelWaitingPopup>())
		{
			_waitingPopupClosedTime += Time.deltaTime;
			if (_waitingPopupClosedTime > 1f)
			{
				SceneAutoBehaviour<DuelManagement>.instance.CancelDuel();
				SingletonUpdater<DuelManager>.instance.CancelCurrentDuel();
			}
		}
	}

	public override void OnBegin()
	{
		SceneAutoBehaviour<DuelManagement>.instance.OpenDuelWaitingPopup(base.VsPlayerId, base.GroupId, DuelWaitingPopup.EWaitingType.RequestDuel);
		int teamLevel = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.TeamLevel;
		string nick = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Nick;
		int avatarHeroIndex = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AvatarHeroIndex;
		NShared.NMessage.NProtocol.RequestDuel requestDuel = new NShared.NMessage.NProtocol.RequestDuel();
		requestDuel.GroupId = base.GroupId;
		requestDuel.Nick = nick;
		requestDuel.AvatarIndex = avatarHeroIndex;
		requestDuel.TeamLevel = teamLevel;
		SingletonAuto<MessageLogHandler>.instance.SendToClient(base.VsPlayerId, requestDuel);
		SingletonUpdater<DuelManager>.instance.LastRequestTickDic[base.VsPlayerId] = UnitySystemTimer.SystemTick;
	}

	public override void OnEnd()
	{
		SceneAutoBehaviour<DuelManagement>.instance.CloseDuelWaitingPopup();
	}

	public override void OnReceive(long senderId, AcceptDuel data)
	{
		if (senderId != base.VsPlayerId || data.GroupId != base.GroupId)
		{
			SingletonUpdater<DuelManager>.instance.SendCancelDuel(senderId, data.GroupId, isReject: false);
		}
		else
		{
			SingletonUpdater<DuelManager>.instance.ChangeState(this, new BattlePartySetting(base.VsPlayerId, base.GroupId));
		}
	}

	public override void OnReceive(long senderId, CancelDuel data)
	{
		if (senderId == base.VsPlayerId && data.GroupId == base.GroupId)
		{
			CenterMessage.Open(LocalizationManager.GetString((!data.IsReject) ? Duel.DUEL_CANCEL : Duel.DUEL_REJECT));
			SceneAutoBehaviour<DuelManagement>.instance.CancelDuel();
			SingletonUpdater<DuelManager>.instance.ChangeState(this, new None());
		}
	}
}
