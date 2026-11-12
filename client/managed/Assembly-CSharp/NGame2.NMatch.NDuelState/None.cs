using NGame2.NMessageLog;
using NGame2.NNotify;
using NGame2.NUI.NManager.NBattle;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NMatch.NDuelState;

public class None : StateBase
{
	public None()
		: base(0L, 0L)
	{
	}

	public override void Request(long vsPlayerId, long groupId)
	{
		SingletonUpdater<DuelManager>.instance.ChangeState(this, new RequestDuel(vsPlayerId, groupId));
	}

	public override void CancelCurrentDuel()
	{
	}

	public override void AcceptDuel(long requestId, long groupId)
	{
		if (SceneAutoBehaviour<BattleManagement>.isValidInstance)
		{
			SceneAutoBehaviour<BattleManagement>.instance.OnAcceptDuel(requestId, groupId);
			return;
		}
		AcceptDuel acceptDuel = new AcceptDuel();
		acceptDuel.GroupId = groupId;
		SingletonAuto<MessageLogHandler>.instance.SendToClient(requestId, acceptDuel);
		SingletonUpdater<DuelManager>.instance.ChangeState(this, new BattlePartySetting(requestId, groupId));
	}

	public override void OnReceive(long senderId, CanDuelRequest data)
	{
		CanDuelResponse canDuelResponse = new CanDuelResponse();
		canDuelResponse.CanDuel = SingletonUpdater<DuelManager>.instance.CanDuel(senderId, isMyRequest: false);
		SingletonAuto<MessageLogHandler>.instance.SendToClient(senderId, canDuelResponse);
	}

	public override void OnReceive(long senderId, NShared.NMessage.NProtocol.RequestDuel data)
	{
		if (!SingletonUpdater<DuelManager>.instance.CanDuel(senderId, isMyRequest: false))
		{
			SingletonUpdater<DuelManager>.instance.SendCancelDuel(senderId, data.GroupId, isReject: false);
			return;
		}
		GlobalAutoBehaviour<InviteNotifyManager>.instance.AddNotify(InviteNotifyType.Duel, data, delegate(bool accept)
		{
			if (accept)
			{
				accept = InviteNotifyManager.IsOpenableNoty(senderId);
			}
			if (accept)
			{
				SingletonUpdater<DuelManager>.instance.AcceptDuel(senderId, data.GroupId);
			}
			else
			{
				SingletonUpdater<DuelManager>.instance.SendCancelDuel(senderId, data.GroupId, isReject: true);
			}
		}, delegate
		{
			SingletonUpdater<DuelManager>.instance.SendCancelDuel(senderId, data.GroupId, isReject: false);
		});
	}
}
