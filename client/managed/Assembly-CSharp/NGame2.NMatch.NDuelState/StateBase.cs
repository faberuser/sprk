using NGame2.NMessageLog;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NMatch.NDuelState;

public abstract class StateBase
{
	public long VsPlayerId { get; private set; }

	public long GroupId { get; private set; }

	public StateBase(long vsPlayerId, long groupId)
	{
		VsPlayerId = vsPlayerId;
		GroupId = groupId;
	}

	public virtual void OnBegin()
	{
	}

	public virtual void OnEnd()
	{
	}

	public virtual void ChainUpdate()
	{
	}

	public virtual void Request(long vsPlayerId, long groupId)
	{
	}

	public virtual void AcceptDuel(long requestId, long groupId)
	{
		SingletonUpdater<DuelManager>.instance.SendCancelDuel(requestId, groupId, isReject: false);
	}

	public virtual void CompletePartySetting(long groupId, int[] heroIndices, int leaderHeroIndex)
	{
		SingletonUpdater<DuelManager>.instance.SendCancelDuel(VsPlayerId, groupId, isReject: false);
		SingletonUpdater<DuelManager>.instance.ChangeState(this, new None());
	}

	public virtual void CancelCurrentDuel()
	{
		SingletonUpdater<DuelManager>.instance.SendCancelDuel(VsPlayerId, GroupId, isReject: false);
		SingletonUpdater<DuelManager>.instance.ChangeState(this, new None());
	}

	public virtual void OnReceive(long senderId, CanDuelRequest data)
	{
		CanDuelResponse canDuelResponse = new CanDuelResponse();
		canDuelResponse.CanDuel = false;
		SingletonAuto<MessageLogHandler>.instance.SendToClient(senderId, canDuelResponse);
	}

	public virtual void OnReceive(long senderId, NShared.NMessage.NProtocol.RequestDuel data)
	{
		SingletonUpdater<DuelManager>.instance.SendCancelDuel(senderId, data.GroupId, isReject: false);
	}

	public virtual void OnReceive(long senderId, AcceptDuel data)
	{
		SingletonUpdater<DuelManager>.instance.SendCancelDuel(senderId, data.GroupId, isReject: false);
	}

	public virtual void OnReceive(long senderId, CancelDuel data)
	{
		if (senderId == VsPlayerId && data.GroupId == GroupId)
		{
			CenterMessage.Open(LocalizationManager.GetString((!data.IsReject) ? Duel.DUEL_CANCEL : Duel.DUEL_REJECT));
			SceneAutoBehaviour<DuelManagement>.instance.CancelDuel();
			SingletonUpdater<DuelManager>.instance.ChangeState(this, new None());
		}
	}

	public virtual void OnReceive(long senderId, CompletePartySetting data)
	{
		SingletonUpdater<DuelManager>.instance.SendCancelDuel(senderId, data.GroupId, isReject: false);
	}

	public virtual void OnReceive(long senderId, NShared.NMessage.NProtocol.BeginDuel data)
	{
		SingletonUpdater<DuelManager>.instance.SendCancelDuel(senderId, data.GroupId, isReject: false);
	}
}
